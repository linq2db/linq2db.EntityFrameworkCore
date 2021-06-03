using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LinqToDB.Expressions;
using LinqToDB.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinqToDB.EntityFrameworkCore
{
	using Mapping;
	using Metadata;
	using Extensions;
	using Common;
	using Internal;
	using SqlQuery;
	using SqlExpression = SqlExpression;

	/// <summary>
	/// LINQ To DB metadata reader for EF.Core model.
	/// </summary>
	internal class EFCoreMetadataReader : IMetadataReader
	{
		readonly IModel? _model;
		private readonly RelationalSqlTranslatingExpressionVisitorDependencies? _dependencies;
		private readonly IRelationalTypeMappingSource? _mappingSource;
		private readonly ConcurrentDictionary<MemberInfo, EFCoreExpressionAttribute?> _calculatedExtensions = new ConcurrentDictionary<MemberInfo, EFCoreExpressionAttribute?>();
		private readonly IDiagnosticsLogger<DbLoggerCategory.Query>? _logger;

		public EFCoreMetadataReader(
			IModel? model,
			RelationalSqlTranslatingExpressionVisitorDependencies? dependencies,
			IRelationalTypeMappingSource? mappingSource,
			IDiagnosticsLogger<DbLoggerCategory.Query>? logger)
		{
			_model = model;
			_dependencies = dependencies;
			_mappingSource = mappingSource;
			_logger = logger;
		}

		public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
		{
			var et = _model?.FindEntityType(type);
			if (et != null)
			{
				if (typeof(T) == typeof(TableAttribute))
				{
					var storeObjectId = GetStoreObjectIdentifier(et);
					return new[] { (T)(Attribute)new TableAttribute(storeObjectId!.Value.Name) { Schema = storeObjectId!.Value.Schema } };
				}
				if (typeof(T) == typeof(QueryFilterAttribute))
				{
					var filter = et.GetQueryFilter();

					if (filter != null)
					{
						var queryParam   = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(type), "q");
						var dcParam      = Expression.Parameter(typeof(IDataContext), "dc");
						var contextProp  = Expression.Property(Expression.Convert(dcParam, typeof(LinqToDBForEFToolsDataConnection)), "Context");
						var filterBody   = filter.Body.Transform(e =>
						{
							if (typeof(DbContext).IsSameOrParentOf(e.Type))
							{
								Expression newExpr = contextProp;
								if (newExpr.Type != e.Type)
									newExpr = Expression.Convert(newExpr, e.Type);
								return newExpr;
							}

							return e;
						});

						// we have found dependency, check for compatibility

						var filterLambda = Expression.Lambda(filterBody, filter.Parameters[0]);
						Expression body  = Expression.Call(Methods.Queryable.Where.MakeGenericMethod(type), queryParam, filterLambda);

						var checkType = filter.Body != filterBody;
						if (checkType)
						{
							body = Expression.Condition(
								Expression.TypeIs(dcParam, typeof(LinqToDBForEFToolsDataConnection)), body, queryParam);
						}

						var lambda       = Expression.Lambda(body, queryParam, dcParam);

						return new[] { (T) (Attribute) new QueryFilterAttribute { FilterFunc = lambda.Compile() } };
					}
				}
			}

			if (typeof(T) == typeof(TableAttribute))
			{
				var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>(inherit);
				if (tableAttribute != null)
					return new[] { (T)(Attribute)new TableAttribute(tableAttribute.Name) { Schema = tableAttribute.Schema } };
			}

			return Array.Empty<T>();
		}

		static bool CompareProperty(MemberInfo? property, MemberInfo memberInfo)
		{
			if (property == memberInfo)
				return true;

			if (property == null)
				return false;
			
			if (memberInfo.DeclaringType?.IsAssignableFrom(property.DeclaringType) == true
			    && memberInfo.Name == property.Name 
			    && memberInfo.MemberType == property.MemberType 
			    && memberInfo.GetMemberType() == property.GetMemberType())
			{
				return true;
			}

			return false;
		}

		static bool CompareProperty(IProperty property, MemberInfo memberInfo)
		{
			return CompareProperty(property.GetIdentifyingMemberInfo(), memberInfo);
		}
		
		public T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute
		{
			if (typeof(Expression).IsSameOrParentOf(type)) 
				return Array.Empty<T>();

			if (typeof(T) == typeof(ColumnAttribute))
			{
				var et = _model?.FindEntityType(type);
				if (et != null)
				{
					var props = et.GetProperties();
					var prop  = props.FirstOrDefault(p => CompareProperty(p, memberInfo));
					
					if (prop != null)
					{
						var isPrimaryKey = prop.IsPrimaryKey();
						var primaryKeyOrder = 0;
						if (isPrimaryKey)
						{
							var pk = prop.FindContainingPrimaryKey();
							primaryKeyOrder = pk.Properties.Select((p, i) => new { p, index = i })
								                  .FirstOrDefault(v => CompareProperty(v.p, memberInfo))?.index ?? 0;
						}

						var isIdentity = prop.GetAnnotations()
							.Any(a =>
							{
								if (a.Name.EndsWith(":ValueGenerationStrategy"))
									return a.Value?.ToString().Contains("Identity") == true;

								if (a.Name.EndsWith(":Autoincrement"))
									return a.Value is bool b && b;

								// for postgres
								if (a.Name == "Relational:DefaultValueSql")
								{
									if (a.Value is string str)
									{
										return str.ToLower().Contains("nextval");
									}
								}

								return false;
							});

						var storeObjectId = GetStoreObjectIdentifier(et);

						return new T[]{(T)(Attribute) new ColumnAttribute
						{
							Name            = prop.GetColumnName(storeObjectId!.Value),
							Length          = prop.GetMaxLength() ?? 0,
							CanBeNull       = prop.IsNullable,
							DbType          = prop.GetColumnType(),
							IsPrimaryKey    = isPrimaryKey,
							PrimaryKeyOrder = primaryKeyOrder,
							IsIdentity      = isIdentity,
						}};
					}
				}

				var columnAttributes = memberInfo.GetCustomAttributes<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>(inherit);

				return columnAttributes.Select(c => (T)(Attribute)new ColumnAttribute
				{
					Name = c.Name,
					DbType = c.TypeName,
				}).ToArray();
			}

			if (typeof(T) == typeof(AssociationAttribute))
			{
				var et = _model?.FindEntityType(type);
				var navigations = et?.GetNavigations().Where(n => CompareProperty(n.PropertyInfo, memberInfo)).ToArray();
				if (navigations?.Length > 0)
				{
					var associations = new List<AssociationAttribute>();
					foreach (var navigation in navigations)
					{
						var fk = navigation.ForeignKey;
						if (!navigation.IsOnDependent)
						{
							var thisKey  = string.Join(",", fk.PrincipalKey.Properties.Select(p => p.Name));
							var otherKey = string.Join(",", fk.Properties.Select(p => p.Name));
							associations.Add(new AssociationAttribute
							{
								ThisKey         = thisKey,
								OtherKey        = otherKey,
								CanBeNull       = !fk.IsRequiredDependent,
								IsBackReference = false
							});
						}
						else
						{
							var thisKey  = string.Join(",", fk.Properties.Select(p => p.Name));
							var otherKey = string.Join(",", fk.PrincipalKey.Properties.Select(p => p.Name));
							associations.Add(new AssociationAttribute
							{
								ThisKey         = thisKey,
								OtherKey        = otherKey,
								CanBeNull       = !fk.IsRequired,
								IsBackReference = true
							});
						}
					}

					return associations.Select(a => (T)(Attribute)a).ToArray();
				}
			} 
			else if (typeof(T) == typeof(Sql.ExpressionAttribute))
			{
				// Search for translator first
				if (_dependencies != null)
				{
					if (memberInfo.IsMethodEx())
					{
						var methodInfo = (MethodInfo) memberInfo;
						var func = GetDbFunctionFromMethodCall(type, methodInfo);
						if (func != null)
							return new T[] { (T) (Attribute) func };
					}
					else if (memberInfo.IsPropertyEx())
					{
						var propertyInfo = (PropertyInfo) memberInfo;
						var func = GetDbFunctionFromProperty(type, propertyInfo);
						if (func != null)
							return new T[] { (T) (Attribute) func };
					}
				}

				if (memberInfo.IsMethodEx())
				{
					var method = (MethodInfo) memberInfo;

					var func = _model?.GetDbFunctions().FirstOrDefault(f => f.MethodInfo == method);
					if (func != null)
						return new T[]
						{
							(T) (Attribute) new Sql.FunctionAttribute
							{
								Name = func.Name,
								ServerSideOnly = true
							}
						};

					var functionAttributes = memberInfo.GetCustomAttributes<DbFunctionAttribute>(inherit);
					return functionAttributes.Select(f => (T) (Attribute) new Sql.FunctionAttribute
					{
						Name = f.Name,
						ServerSideOnly = true,
					}).ToArray();
				}
			}
			else if (typeof(T) == typeof(ValueConverterAttribute))
			{
				var et = _model?.FindEntityType(type);
				if (et != null)
				{
					var props = et.GetProperties();
					var prop  = props.FirstOrDefault(p => CompareProperty(p, memberInfo));

					var converter = prop?.GetValueConverter();
					if (converter != null)
					{
						var valueConverterAttribute = new ValueConverterAttribute
						{
							ValueConverter = new ValueConverter(converter.ConvertToProviderExpression,
								converter.ConvertFromProviderExpression, false)
						};
						return new T[] { (T) (Attribute) valueConverterAttribute };
					}
				}
			}

			return Array.Empty<T>();
		}

		class ValueConverter : IValueConverter
		{
			public ValueConverter(
				LambdaExpression convertToProviderExpression,
				LambdaExpression convertFromProviderExpression, bool handlesNulls)
			{
				FromProviderExpression = convertFromProviderExpression;
				ToProviderExpression   = convertToProviderExpression;
				HandlesNulls           = handlesNulls;
			}

			public bool             HandlesNulls           { get; }
			public LambdaExpression FromProviderExpression { get; }
			public LambdaExpression ToProviderExpression   { get; }
		
		}

		class SqlTransparentExpression : SqlExpression
		{
			public Expression Expression { get; }

			public SqlTransparentExpression(Expression expression, RelationalTypeMapping? typeMapping) : base(expression.Type, typeMapping)
			{
				Expression = expression;
			}

			protected override void Print(ExpressionPrinter expressionPrinter)
			{
				expressionPrinter.Print(Expression);
			}

			protected bool Equals(SqlTransparentExpression other)
			{
				return ReferenceEquals(this, other);
			}

			public override bool Equals(object obj)
			{
				if (obj is null) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((SqlTransparentExpression) obj);
			}

			public override int GetHashCode()
			{
				return RuntimeHelpers.GetHashCode(this);
			}
		}

		private StoreObjectIdentifier? GetStoreObjectIdentifier(IEntityType entityType)
		{
			return entityType.GetTableName() switch
			{
				not null => StoreObjectIdentifier.Create(entityType, StoreObjectType.Table),
				null     => StoreObjectIdentifier.Create(entityType, StoreObjectType.View),
			};
		}

		private Sql.ExpressionAttribute? GetDbFunctionFromMethodCall(Type type, MethodInfo methodInfo)
		{
			if (_dependencies == null || _model == null)
				return null;

			methodInfo = (MethodInfo?) type.GetMemberEx(methodInfo) ?? methodInfo;

			var found = _calculatedExtensions.GetOrAdd(methodInfo, mi =>
			{
				EFCoreExpressionAttribute? result = null;

				if (!methodInfo.IsGenericMethodDefinition && !mi.GetCustomAttributes<Sql.ExpressionAttribute>().Any())
				{
					var value = Expression.Constant(DefaultValue.GetValue(type), type);

					var objExpr = new SqlTransparentExpression(value, _mappingSource?.FindMapping(type));
					var parameterInfos = methodInfo.GetParameters();
					var parametersArray = parameterInfos
						.Select(p =>
							(SqlExpression)new SqlTransparentExpression(
								Expression.Constant(DefaultValue.GetValue(p.ParameterType), p.ParameterType),
								_mappingSource?.FindMapping(p.ParameterType))).ToArray();

					var newExpression = _dependencies.MethodCallTranslatorProvider.Translate(_model, objExpr, methodInfo, parametersArray, _logger);
					if (newExpression != null)
					{
						if (!methodInfo.IsStatic)
							parametersArray = new SqlExpression[] { objExpr }.Concat(parametersArray).ToArray();

						result = ConvertToExpressionAttribute(methodInfo, newExpression, parametersArray);
					}
				}

				return result;
			});

			return found;
		}

		private Sql.ExpressionAttribute? GetDbFunctionFromProperty(Type type, PropertyInfo propInfo)
		{
			if (_dependencies == null || _model == null)
				return null;

			propInfo = (PropertyInfo?) type.GetMemberEx(propInfo) ?? propInfo;

			var found = _calculatedExtensions.GetOrAdd(propInfo, mi =>
			{
				EFCoreExpressionAttribute? result = null;

				if ((propInfo.GetMethod?.IsStatic != true)
				    && !(mi is DynamicColumnInfo)
				    && !mi.GetCustomAttributes<Sql.ExpressionAttribute>().Any())
				{
					var objExpr = new SqlTransparentExpression(Expression.Constant(DefaultValue.GetValue(type), type), _mappingSource?.FindMapping(propInfo));

					var newExpression = _dependencies.MemberTranslatorProvider.Translate(objExpr, propInfo, propInfo.GetMemberType(), _logger);
					if (newExpression != null)
					{
						var parametersArray = new Expression[] { objExpr };
						result = ConvertToExpressionAttribute(propInfo, newExpression, parametersArray);
					}
				}

				return result;
			});

			return found;
		}

		private static EFCoreExpressionAttribute ConvertToExpressionAttribute(MemberInfo memberInfo, Expression newExpression, Expression[] parameters)
		{
			string PrepareExpressionText(Expression? expr)
			{
				var idx = -1;

				for (var index = 0; index < parameters.Length; index++)
				{
					var param = parameters[index];
					var found = ReferenceEquals(expr, param);
					if (!found)
					{
						if (param is SqlTransparentExpression transparent)
						{
							if (transparent.Expression is ConstantExpression &&
							    expr is SqlConstantExpression)
							{
								//found = sqlConstantExpr.Value.Equals(constantExpr.Value);
								found = true;
							}
						}
					}

					if (found)
					{
						idx = index;
						break;
					}
				}

				if (idx >= 0)
					return $"{{{idx}}}";

				if (expr is SqlFragmentExpression fragment)
					return fragment.Sql;

				if (expr is SqlFunctionExpression sqlFunction)
				{
					var text = sqlFunction.Name;
					if (!sqlFunction.Schema.IsNullOrEmpty())
						text = sqlFunction.Schema + "." + sqlFunction.Name;

					if (!sqlFunction.IsNiladic)
					{
						text = text + "(";
						for (var i = 0; i < sqlFunction.Arguments.Count; i++)
						{
							var paramText = PrepareExpressionText(sqlFunction.Arguments[i]);
							if (i > 0)
								text = text + ", ";
							text = text + paramText;
						}

						text = text + ")";
					}

					return text;
				}

				if (newExpression.GetType().Name == "PostgresBinaryExpression")
				{
					// Handling NpgSql's PostgresBinaryExpression

					var left  = newExpression.GetType().GetProperty("Left")?.GetValue(newExpression) as Expression;
					var right = newExpression.GetType().GetProperty("Right")?.GetValue(newExpression) as Expression;

					var operand = newExpression.GetType().GetProperty("OperatorType")?.GetValue(newExpression).ToString();

					var operandExpr = operand switch
					{
						"Contains"
							when left!.Type.Name == "NpgsqlInetTypeMapping" ||
							     left.Type.Name == "NpgsqlCidrTypeMapping"
							=> ">>",
						"ContainedBy"
							when left!.Type.Name == "NpgsqlInetTypeMapping" ||
							     left.Type.Name == "NpgsqlCidrTypeMapping"
							=> "<<",
						"Contains"                      => "@>",
						"ContainedBy"                   => "<@",
						"Overlaps"                      => "&&",
						"AtTimeZone"                    => "AT TIME ZONE",
						"NetworkContainedByOrEqual"     => "<<=",
						"NetworkContainsOrEqual"        => ">>=",
						"NetworkContainsOrContainedBy"  => "&&",
						"RangeIsStrictlyLeftOf"         => "<<",
						"RangeIsStrictlyRightOf"        => ">>",
						"RangeDoesNotExtendRightOf"     => "&<",
						"RangeDoesNotExtendLeftOf"      => "&>",
						"RangeIsAdjacentTo"             => "-|-",
						"RangeUnion"                    => "+",
						"RangeIntersect"                => "*",
						"RangeExcept"                   => "-",
						"TextSearchMatch"               => "@@",
						"TextSearchAnd"                 => "&&",
						"TextSearchOr"                  => "||",
						"JsonExists"                    => "?",
						"JsonExistsAny"                 => "?|",
						"JsonExistsAll"                 => "?&",
						_ => throw new InvalidOperationException(
							$"Unknown PostgresBinaryExpression.OperatorType: '{operand}'")
					};

					switch (operand)
					{
						case "Contains":
							operandExpr = "@>"; break;
						case "ContainedBy":
							operandExpr = "<@"; break;
						case "Overlaps":
							operandExpr = "&&"; break;
					}

					var text = $"{PrepareExpressionText(left)} {operandExpr} {PrepareExpressionText(right)}";

					return text;
				}

				return "NULL";
			}

			var converted = UnwrapConverted(newExpression);
			var expressionText = PrepareExpressionText(converted);
			var result = new EFCoreExpressionAttribute(expressionText)
				{ ServerSideOnly = true, IsPredicate = memberInfo.GetMemberType() == typeof(bool) };

			if (converted is SqlFunctionExpression or SqlFragmentExpression)
				result.Precedence = Precedence.Primary;

			return result;
		}

		private static Expression UnwrapConverted(Expression expr)
		{
			if (expr is SqlFunctionExpression func)
			{
				if (string.Equals(func.Name, "COALESCE", StringComparison.InvariantCultureIgnoreCase) &&
				    func.Arguments.Count == 2 && func.Arguments[1].NodeType == ExpressionType.Extension)
					return UnwrapConverted(func.Arguments[0]);
			}

			return expr;
		}

		public MemberInfo[] GetDynamicColumns(Type type)
		{
			return Array.Empty<MemberInfo>();
		}
	}
}
