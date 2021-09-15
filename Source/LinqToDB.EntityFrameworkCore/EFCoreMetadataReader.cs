using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LinqToDB.Expressions;
using LinqToDB.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
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
		private readonly IMigrationsAnnotationProvider? _annotationProvider;
		private readonly ConcurrentDictionary<MemberInfo, EFCoreExpressionAttribute?> _calculatedExtensions = new();

		public EFCoreMetadataReader(
			IModel? model, IInfrastructure<IServiceProvider>? accessor)
		{
			_model = model;
			if (accessor != null)
			{
				_dependencies       = accessor.GetService<RelationalSqlTranslatingExpressionVisitorDependencies>();
				_mappingSource      = accessor.GetService<IRelationalTypeMappingSource>();
				_annotationProvider = accessor.GetService<IMigrationsAnnotationProvider>();
			}
		}

		public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
		{
			var et = _model?.FindEntityType(type);
			if (et != null)
			{
				if (typeof(T) == typeof(TableAttribute))
				{
					return new[] { (T)(Attribute)new TableAttribute(et.GetTableName()) { Schema = et.GetSchema() } };
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

						filterBody = LinqToDBForEFTools.TransformExpression(filterBody, null, null, _model);

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
			else if (_model != null && typeof(T) == typeof(InheritanceMappingAttribute))
			{
				if (et != null)
				{
					var derivedEntities = _model.GetEntityTypes().Where(e => e.BaseType == et && e.GetDiscriminatorValue() != null).ToList();

					return
						derivedEntities.Select(e =>
								(T)(Attribute)new InheritanceMappingAttribute
								{
									Type = e.ClrType, 
									Code = e.GetDiscriminatorValue()
								}
							)
							.ToArray();
				}

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

		static DataType DbTypeToDataType(DbType dbType)
		{
			return dbType switch
			{
				DbType.AnsiString => DataType.VarChar,
				DbType.AnsiStringFixedLength => DataType.VarChar,
				DbType.Binary => DataType.Binary,
				DbType.Boolean => DataType.Boolean,
				DbType.Byte => DataType.Byte,
				DbType.Currency => DataType.Money,
				DbType.Date => DataType.Date,
				DbType.DateTime => DataType.DateTime,
				DbType.DateTime2 => DataType.DateTime2,
				DbType.DateTimeOffset => DataType.DateTimeOffset,
				DbType.Decimal => DataType.Decimal,
				DbType.Double => DataType.Double,
				DbType.Guid => DataType.Guid,
				DbType.Int16 => DataType.Int16,
				DbType.Int32 => DataType.Int32,
				DbType.Int64 => DataType.Int64,
				DbType.Object => DataType.Undefined,
				DbType.SByte => DataType.SByte,
				DbType.Single => DataType.Single,
				DbType.String => DataType.NVarChar,
				DbType.StringFixedLength => DataType.NVarChar,
				DbType.Time => DataType.Time,
				DbType.UInt16 => DataType.UInt16,
				DbType.UInt32 => DataType.UInt32,
				DbType.UInt64 => DataType.UInt64,
				DbType.VarNumeric => DataType.VarNumeric,
				DbType.Xml => DataType.Xml,
				_ => DataType.Undefined
			};
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
						var discriminator = et.GetDiscriminatorProperty();

						var isPrimaryKey = prop.IsPrimaryKey();
						var primaryKeyOrder = 0;
						if (isPrimaryKey)
						{
							var pk = prop.FindContainingPrimaryKey()!;
							primaryKeyOrder = pk.Properties.Select((p, i) => new { p, index = i })
								                  .FirstOrDefault(v => CompareProperty(v.p, memberInfo))?.index ?? 0;
						}

						var annotations = prop.GetAnnotations();
						if (_annotationProvider != null)
						{
							annotations = annotations.Concat(_annotationProvider.For(prop));
						}

						var isIdentity = annotations
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

						var dataType = DataType.Undefined;

						if (prop.GetTypeMapping() is RelationalTypeMapping typeMapping)
						{
							if (typeMapping.DbType != null)
							{
								dataType = DbTypeToDataType(typeMapping.DbType.Value);
							}
							else
							{
								dataType = SqlDataType.GetDataType(typeMapping.ClrType).Type.DataType;
							}
						}

						return new T[]
						{
							(T)(Attribute)new ColumnAttribute
							{
								Name            = prop.GetColumnName(),
								Length          = prop.GetMaxLength() ?? 0,
								CanBeNull       = prop.IsNullable,
								DbType          = prop.GetColumnType(),
								DataType        = dataType,
								IsPrimaryKey    = isPrimaryKey,
								PrimaryKeyOrder = primaryKeyOrder,
								IsIdentity      = isIdentity,
								IsDiscriminator = discriminator == prop
							}
						};
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
						if (!navigation.IsDependentToPrincipal())
						{
							// Could not track when EF decides to do INNER JOIN
							var canBeNull = true;

							var thisKey  = string.Join(",", fk.PrincipalKey.Properties.Select(p => p.Name));
							var otherKey = string.Join(",", fk.Properties.Select(p => p.Name));
							associations.Add(new AssociationAttribute
							{
								ThisKey         = thisKey,
								OtherKey        = otherKey,
								CanBeNull       = canBeNull,
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

			public override void Print(ExpressionPrinter expressionPrinter)
			{
				expressionPrinter.Print(Expression);
			}

			protected bool Equals(SqlTransparentExpression other)
			{
				return ReferenceEquals(this, other);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((SqlTransparentExpression) obj);
			}

			public override int GetHashCode()
			{
				return RuntimeHelpers.GetHashCode(this);
			}
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

					var newExpression = _dependencies.MethodCallTranslatorProvider.Translate(_model, objExpr, methodInfo, parametersArray);
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

					var newExpression = _dependencies.MemberTranslatorProvider.Translate(objExpr, propInfo, propInfo.GetMemberType());
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
							if (transparent.Expression is ConstantExpression constantExpr &&
							    expr is SqlConstantExpression sqlConstantExpr)
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
						for (int i = 0; i < sqlFunction.Arguments.Count; i++)
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

				if (newExpression.GetType().GetProperty("Left") != null &&
				    newExpression.GetType().GetProperty("Right") != null &&
				    newExpression.GetType().GetProperty("Operator") != null)
				{
					// Handling NpgSql's CustomBinaryExpression

					var left = newExpression.GetType().GetProperty("Left")?.GetValue(newExpression) as Expression;
					var right = newExpression.GetType().GetProperty("Right")?.GetValue(newExpression) as Expression;

					var operand = newExpression.GetType().GetProperty("Operator")?.GetValue(newExpression) as string;

					var text = $"{PrepareExpressionText(left)} {operand} {PrepareExpressionText(right)}";

					return text;
				}

				return "NULL";
			}

			var converted = UnwrapConverted(newExpression);
			var expressionText = PrepareExpressionText(converted);
			var result = new EFCoreExpressionAttribute(expressionText)
				{ ServerSideOnly = true, IsPredicate = memberInfo.GetMemberType() == typeof(bool) };

			if (converted is SqlFunctionExpression || converted is SqlFragmentExpression)
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
