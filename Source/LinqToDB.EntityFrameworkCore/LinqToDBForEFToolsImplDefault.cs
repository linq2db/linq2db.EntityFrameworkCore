using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

using JetBrains.Annotations;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;
	using Expressions;
	using Mapping;
	using Metadata;
	using Extensions;
	using SqlQuery;
	using Common.Internal.Cache;

	using DataProvider;
	using DataProvider.DB2;
	using DataProvider.Firebird;
	using DataProvider.MySql;
	using DataProvider.Oracle;
	using DataProvider.PostgreSQL;
	using DataProvider.SQLite;
	using DataProvider.SqlServer;
	using DataProvider.SqlCe;

	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	/// <summary>
	/// Default EF.Core - LINQ To DB integration bridge implementation.
	/// </summary>
	[PublicAPI]
	public class LinqToDBForEFToolsImplDefault : ILinqToDBForEFTools
	{
		class ProviderKey
		{
			public ProviderKey(string providerName, string connectionString)
			{
				ProviderName = providerName;
				ConnectionString = connectionString;
			}

			string ProviderName { get; }
			string ConnectionString { get; }

			#region Equality members

			protected bool Equals(ProviderKey other)
			{
				return string.Equals(ProviderName, other.ProviderName) && string.Equals(ConnectionString, other.ConnectionString);
			}

			public override bool Equals(object obj)
			{
				if (obj is null) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((ProviderKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((ProviderName != null ? ProviderName.GetHashCode() : 0) * 397) ^ (ConnectionString != null ? ConnectionString.GetHashCode() : 0);
				}
			}
			
			#endregion
		}

		readonly ConcurrentDictionary<ProviderKey, IDataProvider> _knownProviders = new ConcurrentDictionary<ProviderKey, IDataProvider>();

		private readonly MemoryCache _schemaCache = new MemoryCache(
			new MemoryCacheOptions
			{
				ExpirationScanFrequency = TimeSpan.FromHours(1.0)
			});


		public virtual void ClearCaches()
		{
			_knownProviders.Clear();
			_schemaCache.Compact(1.0);
		}

		/// <summary>
		/// Returns LINQ To DB provider, based on provider data from EF.Core.
		/// Could be overriden if you have issues with default detection mechanisms.
		/// </summary>
		/// <param name="providerInfo">Provider information, extracted from EF.Core.</param>
		/// <param name="connectionInfo"></param>
		/// <returns>LINQ TO DB provider instance.</returns>
		public virtual IDataProvider GetDataProvider(EFProviderInfo providerInfo, EFConnectionInfo connectionInfo)
		{
			var info = GetLinqToDbProviderInfo(providerInfo);

			return _knownProviders.GetOrAdd(new ProviderKey(info.ProviderName, connectionInfo.ConnectionString), k =>
			{
				return CreateLinqToDbDataProvider(providerInfo, info, connectionInfo);
			});
		}

		protected virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(EFProviderInfo providerInfo)
		{
			var provInfo = new LinqToDBProviderInfo();

			var relational = providerInfo.Options?.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			if (relational != null)
			{
				provInfo.Merge(GetLinqToDbProviderInfo(relational));
			}

			if (providerInfo.Connection != null)
			{
				provInfo.Merge(GetLinqToDbProviderInfo(providerInfo.Connection));
			}

			if (providerInfo.Context != null)
			{
				provInfo.Merge(GetLinqToDbProviderInfo(providerInfo.Context.Database));
			}

			return provInfo;
		}

		protected virtual IDataProvider CreateLinqToDbDataProvider(EFProviderInfo providerInfo, LinqToDBProviderInfo provInfo,
			EFConnectionInfo connectionInfo)
		{
			if (provInfo.ProviderName == null)
			{
				throw new LinqToDBForEFToolsException("Can not detect data provider.");
			}

			switch (provInfo.ProviderName)
			{
					case ProviderName.SqlServer:
						return CreateSqlServerProvider(SqlServerDefaultVersion, connectionInfo.ConnectionString);
					case ProviderName.MySql:
					case ProviderName.MySqlConnector:
						return new MySqlDataProvider(provInfo.ProviderName);
					case ProviderName.PostgreSQL:
						return CreatePostgreSqlProvider(PostgreSqlDefaultVersion, connectionInfo.ConnectionString);
					case ProviderName.SQLite:
						return new SQLiteDataProvider();
					case ProviderName.Firebird:
						return new FirebirdDataProvider();
					case ProviderName.DB2:
						return new DB2DataProvider(ProviderName.DB2, DB2Version.LUW);
					case ProviderName.DB2LUW:
						return new DB2DataProvider(ProviderName.DB2, DB2Version.LUW);
					case ProviderName.DB2zOS:
						return new DB2DataProvider(ProviderName.DB2, DB2Version.zOS);
					case ProviderName.Oracle:
						return new OracleDataProvider();
					case ProviderName.SqlCe:
						return new SqlCeDataProvider();
					//case ProviderName.Access:
					//	return new AccessDataProvider();

			default:
				throw new LinqToDBForEFToolsException($"Can not instantiate data provider '{provInfo.ProviderName}'.");
			}
		}

		protected virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(DatabaseFacade database)
		{
			switch (database.ProviderName)
			{
				case "Microsoft.EntityFrameworkCore.SqlServer":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlServer };

				case "Pomelo.EntityFrameworkCore.MySql":
				case "Devart.Data.MySql.EFCore":
				{
					return new LinqToDBProviderInfo { ProviderName = ProviderName.MySqlConnector };
				}

				case "MySql.Data.EntityFrameworkCore":
				{
					return new LinqToDBProviderInfo { ProviderName = ProviderName.MySql };
				}

				case "Npgsql.EntityFrameworkCore.PostgreSQL":
				case "Devart.Data.PostgreSql.EFCore":
				{
					return new LinqToDBProviderInfo { ProviderName = ProviderName.PostgreSQL };
				}

				case "Microsoft.EntityFrameworkCore.Sqlite":
				case "Devart.Data.SQLite.EFCore":
				{
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SQLite };
				}

				case "FirebirdSql.EntityFrameworkCore.Firebird":
				case "EntityFrameworkCore.FirebirdSql":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Firebird };

				case "IBM.EntityFrameworkCore":
				case "IBM.EntityFrameworkCore-lnx":
				case "IBM.EntityFrameworkCore-osx":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2LUW };
				case "Devart.Data.Oracle.EFCore":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Oracle };
				case "EntityFrameworkCore.Jet":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Access };

				case "EntityFrameworkCore.SqlServerCompact40":
				case "EntityFrameworkCore.SqlServerCompact35":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlCe };
			}

			return null;
		}

		protected virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(DbConnection connection)
		{
			switch (connection.GetType().Name)
			{
					case "SqlConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlServer };
					case "MySqlConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.MySql };
					case "NpgsqlConnection":
					case "PgSqlConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.PostgreSQL };
					case "FbConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.Firebird };
					case "DB2Connection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2LUW };
					case "OracleConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.Oracle };
					case "SqliteConnection":
					case "SQLiteConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.SQLite };
					case "JetConnection":
						return new LinqToDBProviderInfo { ProviderName = ProviderName.Access };
			}

			return null;
		}

		protected  virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(RelationalOptionsExtension extensions)
		{
			switch (extensions.GetType().Name)
			{
				case "MySqlOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.MySqlConnector };
				case "MySQLOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.MySql };
				case "NpgsqlOptionsExtension":
				case "PgSqlOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.PostgreSQL };
				case "SqlServerOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlServer };
				case "SqliteOptionsExtension":
				case "SQLiteOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SQLite };
				case "SqlCeOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlCe };
				case "FbOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Firebird };
				case "Db2OptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2LUW };
				case "OracleOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Oracle };
				case "JetOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.Access };
			}

			return null;
		}

		protected virtual IDataProvider CreateSqlServerProvider(SqlServerVersion version, string connectionString)
		{
			if (!string.IsNullOrEmpty(connectionString))
				return DataConnection.GetDataProvider("System.Data.SqlClient", connectionString);

			string providerName;
			switch (version)
			{
				case SqlServerVersion.v2000:
					providerName = ProviderName.SqlServer2000;
					break;
				case SqlServerVersion.v2005:
					providerName = ProviderName.SqlServer2005;
					break;
				case SqlServerVersion.v2008:
					providerName = ProviderName.SqlServer2008;
					break;
				case SqlServerVersion.v2012:
					providerName = ProviderName.SqlServer2012;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return new SqlServerDataProvider(providerName, version);
		}

		protected virtual IDataProvider CreatePostgreSqlProvider(PostgreSQLVersion version, string connectionString)
		{
			if (!string.IsNullOrEmpty(connectionString))
				return DataConnection.GetDataProvider(ProviderName.PostgreSQL, connectionString);

			string providerName;
			switch (version)
			{
				case PostgreSQLVersion.v92:
					providerName = ProviderName.PostgreSQL92;
					break;
				case PostgreSQLVersion.v93:
					providerName = ProviderName.PostgreSQL93;
					break;
				case PostgreSQLVersion.v95:
					providerName = ProviderName.PostgreSQL95;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(version), version, null);
			}

			return new PostgreSQLDataProvider(providerName, version);
		}

		/// <summary>
		/// Creates metadata provider for specified EF.Core data model. Default implementation uses
		/// <see cref="EFCoreMetadataReader"/> metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="dependencies"></param>
		/// <param name="mappingSource"></param>
		/// <returns>LINQ To DB metadata provider for specified EF.Core model.</returns>
		public virtual IMetadataReader CreateMetadataReader(IModel model,
			RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
			IRelationalTypeMappingSource mappingSource)
		{
			return new EFCoreMetadataReader(model, dependencies, mappingSource);
		}

		/// <summary>
		/// Creates mapping schema using provided EF.Core data model and metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="metadataReader">Additional optional LINQ To DB database metadata provider.</param>
		/// <param name="convertorSelector"></param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		public virtual MappingSchema CreateMappingSchema(IModel model, IMetadataReader metadataReader,
			IValueConverterSelector convertorSelector)
		{
			var schema = new MappingSchema();
			if (metadataReader != null)
				schema.AddMetadataReader(metadataReader);

			DefineConvertors(schema, model, convertorSelector);

			return schema;
		}

		public virtual void DefineConvertors(
			[JetBrains.Annotations.NotNull] MappingSchema mappingSchema,
			[JetBrains.Annotations.NotNull] IModel model, 
			IValueConverterSelector convertorSelector)
		{
			if (mappingSchema == null) throw new ArgumentNullException(nameof(mappingSchema));
			if (model == null)         throw new ArgumentNullException(nameof(model));

			if (convertorSelector == null)
				return;

			var entities = model.GetEntityTypes().ToArray();

			var types = entities.SelectMany(e => e.GetProperties().Select(p => p.ClrType))
				.Distinct()
				.ToArray();

			foreach (var clrType in types)
			{
				var currentType = mappingSchema.GetDataType(clrType);
				if (currentType != SqlDataType.Undefined)
					continue;

				var infos = convertorSelector.Select(clrType).ToArray();
				if (infos.Length > 0)
				{
					foreach (var info in infos)
					{
						currentType = mappingSchema.GetDataType(info.ModelClrType);
						if (currentType != SqlDataType.Undefined)
							continue;

						var dataType    = mappingSchema.GetDataType(info.ProviderClrType);
						var fromParam   = Expression.Parameter(clrType, "t");

						var convertExpression = mappingSchema.GetConvertExpression(clrType, info.ProviderClrType, false);
						var converter         = convertExpression.GetBody(fromParam);

						var valueExpression   = converter;

						if (clrType.IsClass || clrType.IsInterface)
						{
							valueExpression = Expression.Condition(
								Expression.Equal(fromParam,
									Expression.Constant(null, clrType)),
								Expression.Constant(null, clrType),
								valueExpression
							);
						}
						else if (typeof(Nullable<>).IsSameOrParentOf(clrType))
						{
							valueExpression = Expression.Condition(
								Expression.Property(fromParam, "HasValue"),
								Expression.Convert(valueExpression, typeof(object)),
								Expression.Constant(null, typeof(object))
							);
						}

						if (valueExpression.Type != typeof(object))
							valueExpression = Expression.Convert(valueExpression, typeof(object));

						var convertLambda = Expression.Lambda(
							Expression.New(DataParameterConstructor,
								Expression.Constant("Conv", typeof(string)),
								valueExpression,
								Expression.Constant(dataType.DataType, typeof(DataType)),
								Expression.Constant(dataType.DbType,   typeof(string))
							), fromParam);

						mappingSchema.SetConvertExpression(clrType, typeof(DataParameter), convertLambda, false);
					}
				}
			}

		}

		/// <summary>
		/// Returns mapping schema using provided EF.Core data model and metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="metadataReader">Additional optional LINQ To DB database metadata provider.</param>
		/// <param name="convertorSelector"></param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		public virtual MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader,
			IValueConverterSelector convertorSelector)
		{
			var result = _schemaCache.GetOrCreate(Tuple.Create(model, metadataReader, convertorSelector), e =>
			{
				e.SlidingExpiration = TimeSpan.FromHours(1); 
				return CreateMappingSchema(model, metadataReader, convertorSelector);
			});

			return result;
		}

		/// <summary>
		/// Returns EF.Core <see cref="IDbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="IDbContextOptions"/> instance.</returns>
		public virtual IDbContextOptions GetContextOptions(DbContext context)
		{
			return context?.GetService<IDbContextOptions>();
		}

		static readonly MethodInfo GetTableMethodInfo =
			MemberHelper.MethodOf<IDataContext>(dc => dc.GetTable<object>()).GetGenericMethodDefinition();

		static readonly MethodInfo LoadWithMethodInfo = MemberHelper.MethodOf(() => LinqExtensions.LoadWith<int>(null, null)).GetGenericMethodDefinition();

		static readonly MethodInfo WhereMethodInfo =
			MemberHelper.MethodOf<IQueryable<object>>(q => q.Where(p => true)).GetGenericMethodDefinition();

		static readonly MethodInfo IgnoreQueryFiltersMethodInfo =
			MemberHelper.MethodOf<IQueryable<object>>(q => q.IgnoreQueryFilters()).GetGenericMethodDefinition();

		static readonly MethodInfo IncludeMethodInfo =
			MemberHelper.MethodOf<IQueryable<object>>(q => q.Include(o => o.ToString())).GetGenericMethodDefinition();

		static readonly MethodInfo IncludeMethodInfoString =
			MemberHelper.MethodOf<IQueryable<object>>(q => q.Include(string.Empty)).GetGenericMethodDefinition();

		static readonly MethodInfo ThenIncludeMethodInfo =
			MemberHelper.MethodOf<IIncludableQueryable<object, object>>(q => q.ThenInclude<object, object, object>(null)).GetGenericMethodDefinition();

		static readonly MethodInfo ThenIncludeEnumerableMethodInfo =
			MemberHelper.MethodOf<IIncludableQueryable<object, IEnumerable<object>>>(q => q.ThenInclude<object, object, object>(null)).GetGenericMethodDefinition();


		static readonly MethodInfo FirstMethodInfo =
			MemberHelper.MethodOf<IEnumerable<object>>(q => q.First()).GetGenericMethodDefinition();

		static readonly MethodInfo AsNoTrackingMethodInfo =
			MemberHelper.MethodOf<IQueryable<object>>(q => q.AsNoTracking()).GetGenericMethodDefinition();

		static readonly MethodInfo EFProperty =
			MemberHelper.MethodOf(() => EF.Property<object>(1, "")).GetGenericMethodDefinition();

		static readonly MethodInfo
			L2DBProperty = typeof(Sql).GetMethod(nameof(Sql.Property)).GetGenericMethodDefinition();

		static readonly ConstructorInfo DataParameterConstructor = MemberHelper.ConstructorOf(() => new DataParameter("", "", DataType.Undefined, ""));

		public static Expression Unwrap(Expression ex)
		{
			if (ex == null)
				return null;

			switch (ex.NodeType)
			{
				case ExpressionType.Quote          : return Unwrap(((UnaryExpression)ex).Operand);
				case ExpressionType.ConvertChecked :
				case ExpressionType.Convert        :
					{
						var ue = (UnaryExpression)ex;

						if (!ue.Operand.Type.IsEnumEx())
							return Unwrap(ue.Operand);

						break;
					}
			}

			return ex;
		}

		public static bool IsQueryable(MethodCallExpression method, bool enumerable = true)
		{
			var type = method.Method.DeclaringType;

			return type == typeof(Queryable) || (enumerable && type == typeof(Enumerable)) || type == typeof(LinqExtensions) ||
				   type == typeof(EntityFrameworkQueryableExtensions);
		}

		public static object EvaluateExpression(Expression expr)
		{
			if (expr == null)
				return null;

			switch (expr.NodeType)
			{
				case ExpressionType.Constant:
					return ((ConstantExpression)expr).Value;

				case ExpressionType.MemberAccess:
					{
						var member = (MemberExpression) expr;

						if (member.Member.IsFieldEx())
							return ((FieldInfo)member.Member).GetValue(EvaluateExpression(member.Expression));

						if (member.Member.IsPropertyEx())
							return ((PropertyInfo)member.Member).GetValue(EvaluateExpression(member.Expression), null);

						break;
					}
			}

			var value = Expression.Lambda(expr).Compile().DynamicInvoke();
			return value;
		}

		/// <summary>
		/// Compacts expression to handle big filters.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns>Compacted expression.</returns>
		public static Expression CompactExpression(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Or:
				case ExpressionType.And:
				case ExpressionType.OrElse:
				case ExpressionType.AndAlso:
				{
					var stack = new Stack<Expression>();
					var items = new List<Expression>();
					var binary = (BinaryExpression) expression;

					stack.Push(binary.Right);
					stack.Push(binary.Left);
					while (stack.Count > 0)
					{
						var item = stack.Pop();
						if (item.NodeType == expression.NodeType)
						{
							binary = (BinaryExpression) item;
							stack.Push(binary.Right);
							stack.Push(binary.Left);
						}
						else
							items.Add(item);
					}

					if (items.Count > 3)
					{
						// having N items will lead to NxM recursive calls in expression visitors and
						// will result in stack overflow on relatively small numbers (~1000 items).
						// To fix it we will rebalance condition tree here which will result in
						// LOG2(N)*M recursive calls, or 10*M calls for 1000 items.
						//
						// E.g. we have condition A OR B OR C OR D OR E
						// as an expression tree it represented as tree with depth 5
						//   OR
						// A    OR
						//    B    OR
						//       C    OR
						//          D    E
						// for rebalanced tree it will have depth 4
						//                  OR
						//        OR
						//   OR        OR        OR
						// A    B    C    D    E    F
						// Not much on small numbers, but huge improvement on bigger numbers
						while (items.Count != 1)
						{
							items = CompactTree(items, expression.NodeType);
						}

						return items[0];
					}

					break;
				}
			}

			return expression;
		}

		static List<Expression> CompactTree(List<Expression> items, ExpressionType nodeType)
		{
			var result = new List<Expression>();

			// traverse list from left to right to preserve calculation order
			for (var i = 0; i < items.Count; i += 2)
			{
				if (i + 1 == items.Count)
				{
					// last non-paired item
					result.Add(items[i]);
				}
				else
				{
					result.Add(Expression.MakeBinary(nodeType, items[i], items[i + 1]));
				}
			}

			return result;
		}

		/// <summary>
		/// Transforms EF.Core expression tree to LINQ To DB expression.
		/// Method replaces EF.Core <see cref="EntityQueryable{TResult}"/> instances with LINQ To DB
		/// <see cref="DataExtensions.GetTable{T}(IDataContext)"/> calls.
		/// </summary>
		/// <param name="expression">EF.Core expression tree.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <param name="ctx">Optional DbContext instance.</param>
		/// <param name="model">EF.Core data model instance.</param>
		/// <returns>Transformed expression.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
		public virtual Expression TransformExpression(Expression expression, IDataContext dc, DbContext ctx, IModel model)
		{
			var ignoreQueryFilters = false;

			var getTableCalls = new Dictionary<Type, List<List<Expression>>>();
			var currentPropPath = new List<Expression>();

			void RegisterIncludeCall(Type type)
			{
				if (typeof(IQueryable<>).IsSameOrParentOf(type))
				{
					type = type.GenericTypeArguments[0];
				}

				if (!getTableCalls.TryGetValue(type, out var list))
				{
					list = new List<List<Expression>>();
					getTableCalls.Add(type, list);
				}

				list.Add(currentPropPath.ToList());
			}

			Expression GetTableExpression(Type entityType)
			{
				var newExpr = Expression.Call(null, GetTableMethodInfo.MakeGenericMethod(entityType), Expression.Constant(dc));
				if (getTableCalls.TryGetValue(entityType, out var list))
				{
					foreach (var path in list)
					{
						var param = Expression.Parameter(entityType);
						Expression memberExpression = param;

						for (var index = path.Count - 1; index >= 0; index--)
						{
							if (typeof(IEnumerable<>).IsSameOrParentOf(memberExpression.Type))
							{
								memberExpression = Expression.Call(null, FirstMethodInfo.MakeGenericMethod(memberExpression.Type.GenericTypeArguments[0]), memberExpression);
							}

							var prop = Unwrap(path[index]);
							if (prop is LambdaExpression lambda)
							{
								memberExpression = Expression.MakeMemberAccess(memberExpression,
									((MemberExpression) lambda.Body).Member);
							}
							else
							{
								// Navigation path
								if (EvaluateExpression(prop) is string navigationPath)
								{
									var props = navigationPath.Split('.');
									for (int i = 0; i < props.Length; i++)
									{
										var propertyInfo = memberExpression.Type.GetPropertyEx(props[i]);
										if (propertyInfo != null)
											memberExpression = Expression.MakeMemberAccess(memberExpression, propertyInfo);
									}
								}
							}
						}

						if (memberExpression != param)
						{
							if (memberExpression.Type != typeof(object))
							{
								memberExpression = Expression.Convert(memberExpression, typeof(object));
							}

							var loadWithLambda = Expression.Lambda(memberExpression, param);

							newExpr = Expression.Call(null, LoadWithMethodInfo.MakeGenericMethod(entityType), newExpr,
								Expression.Quote(loadWithLambda));
						}
					}
				}
				return newExpr;
			}

			Expression LocalTransform(Expression e)
			{
				e = CompactExpression(e);

				switch (e.NodeType)
				{
					case ExpressionType.Constant:
					{
						if (typeof(EntityQueryable<>).IsSameOrParentOf(e.Type))
						{
							var entityType = e.Type.GenericTypeArguments[0];
							var newExpr = GetTableExpression(entityType);

							if (!ignoreQueryFilters)
							{
								var filter = model?.FindEntityType(entityType).GetQueryFilter();
								if (filter != null)
								{
									var filterBody = filter.Body.Transform(l => LocalTransform(l));

									// replacing DbContext constant
									if (ctx != null)
									{
										filterBody = filterBody.Transform(fe =>
										{
											if (fe.NodeType == ExpressionType.Constant)
											{
												if (fe.Type.IsAssignableFrom(ctx.GetType()))
												{
													return Expression.Constant(ctx, fe.Type);
												}
											}

											return fe;
										});
									}

									filter = Expression.Lambda(filterBody, filter.Parameters[0]);
									var whereExpr = Expression.Call(null, WhereMethodInfo.MakeGenericMethod(entityType), newExpr, Expression.Quote(filter));

									newExpr = whereExpr;
								}
							}

							return newExpr;
						}

						break;
					}

					case ExpressionType.Call:
					{
						var methodCall = (MethodCallExpression) e;
						var generic = methodCall.Method.IsGenericMethod ? methodCall.Method.GetGenericMethodDefinition() : methodCall.Method;

						if (IsQueryable(methodCall))
						{
							if (methodCall.Method.IsGenericMethod)
							{
								var isTunnel = false;

								if (generic == IgnoreQueryFiltersMethodInfo)
								{
									ignoreQueryFilters = true;
									isTunnel = true;
								}
								else if (generic == AsNoTrackingMethodInfo)
									isTunnel = true;
								else if (generic == IncludeMethodInfo || generic == IncludeMethodInfoString)
								{
									currentPropPath.Add(methodCall.Arguments[1]);
									RegisterIncludeCall(methodCall.Method.GetGenericArguments()[0]);
									currentPropPath.Clear();
									isTunnel = true;
								}
								else if (generic == ThenIncludeMethodInfo || generic == ThenIncludeEnumerableMethodInfo)
								{
									currentPropPath.Add(methodCall.Arguments[1]);
									isTunnel = true;
								}

								if (isTunnel)
									return methodCall.Arguments[0].Transform(l => LocalTransform(l));
							}

							break;
						}

						if (typeof(IQueryable<>).IsSameOrParentOf(methodCall.Type))
						{
							// Invoking function to evaluate EF's Subquery located in function

							var obj = EvaluateExpression(methodCall.Object);
							var arguments = methodCall.Arguments.Select(EvaluateExpression).ToArray();
							if (methodCall.Method.Invoke(obj, arguments) is IQueryable result)
							{
								if (!ExpressionEqualityComparer.Instance.Equals(methodCall, result.Expression))
									return result.Expression.Transform(l => LocalTransform(l));
							}
						}

						if (generic == EFProperty)
						{
							var prop = Expression.Call(null, L2DBProperty.MakeGenericMethod(methodCall.Method.GetGenericArguments()[0]),
								methodCall.Arguments[0].Transform(l => LocalTransform(l)), methodCall.Arguments[1]);
							return prop;
						}

						break;
					}
				}

				return e;
			}

			var newExpression = expression.Transform(e => LocalTransform(e));

			return newExpression;
		}

		/// <summary>
		/// Extracts <see cref="DbContext"/> instance from <see cref="IQueryable"/> object.
		/// Due to unavailability of integration API in EF.Core this method use reflection and could became broken after EF.Core update.
		/// </summary>
		/// <param name="query">EF.Core query.</param>
		/// <returns>Current <see cref="DbContext"/> instance.</returns>
		public virtual DbContext GetCurrentContext(IQueryable query)
		{
			var compilerField = typeof (EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);
			var compiler = (QueryCompiler) compilerField.GetValue(query.Provider);

			var queryContextFactoryField = compiler.GetType().GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new LinqToDBForEFToolsException($"Can not find private field '{compiler.GetType()}._queryContextFactory' in current EFCore Version.");

			if (!(queryContextFactoryField.GetValue(compiler) is RelationalQueryContextFactory queryContextFactory))
				throw new LinqToDBForEFToolsException("LinqToDB Tools for EFCore support only Relational Databases.");

			var dependenciesProperty = typeof(RelationalQueryContextFactory).GetField("_dependencies", BindingFlags.NonPublic | BindingFlags.Instance);

			if (dependenciesProperty == null)
				throw new LinqToDBForEFToolsException($"Can not find private property '{nameof(RelationalQueryContextFactory)}._dependencies' in current EFCore Version.");

			var dependencies = (QueryContextDependencies) dependenciesProperty.GetValue(queryContextFactory);

			return dependencies.CurrentContext?.Context;
		}

		/// <summary>
		/// Extracts EF.Core connection information object from <see cref="IDbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="IDbContextOptions"/> instance.</param>
		/// <returns>EF.Core connection data.</returns>
		public virtual EFConnectionInfo ExtractConnectionInfo(IDbContextOptions options)
		{
			var relational = options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			return new  EFConnectionInfo
			{
				ConnectionString = relational?.ConnectionString,
				Connection = relational?.Connection
			};
		}

		/// <summary>
		/// Extracts EF.Core data model instance from <see cref="IDbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="IDbContextOptions"/> instance.</param>
		/// <returns>EF.Core data model instance.</returns>
		public virtual IModel ExtractModel(IDbContextOptions options)
		{
			var coreOptions = options?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault();
			return coreOptions?.Model;
		}

		static int _messageCounter;

		public virtual void LogConnectionTrace(TraceInfo info, ILogger logger)
		{
			Interlocked.Increment(ref _messageCounter);
			switch (info.TraceInfoStep)
			{
				case TraceInfoStep.BeforeExecute:
					logger.LogInformation(_messageCounter, $"{info.TraceInfoStep}{Environment.NewLine}{info.SqlText}");
					break;

				case TraceInfoStep.AfterExecute:
					logger.LogInformation(_messageCounter,
						info.RecordsAffected != null
							? $"Query Execution Time ({info.TraceInfoStep}) {(info.IsAsync ? " (async)" : "")}: {info.ExecutionTime}. Records Affected: {info.RecordsAffected}.\r\n"
							: $"Query Execution Time ({info.TraceInfoStep}) {(info.IsAsync ? " (async)" : "")}: {info.ExecutionTime}\r\n");
					break;

				case TraceInfoStep.Error:
				{
					var sb = new StringBuilder();

					sb.Append(info.TraceInfoStep);

					for (var ex = info.Exception; ex != null; ex = ex.InnerException)
					{
						try
						{
							sb
								.AppendLine()
								.AppendLine($"Exception: {ex.GetType()}")
								.AppendLine($"Message  : {ex.Message}")
								.AppendLine(ex.StackTrace)
								;
						}
						catch
						{
							// Sybase provider could generate exception that will throw another exception when you
							// try to access Message property due to bug in AseErrorCollection.Message property.
							// There it tries to fetch error from first element of list without checking wether
							// list contains any elements or not
							sb
								.AppendLine()
								.AppendFormat("Failed while tried to log failure of type {0}", ex.GetType())
								;
						}
					}

					logger.LogError(_messageCounter, sb.ToString());

					break;
				}

				case TraceInfoStep.Completed:
				{
					var sb = new StringBuilder();

					sb.Append($"Total Execution Time ({info.TraceInfoStep}){(info.IsAsync ? " (async)" : "")}: {info.ExecutionTime}.");

					if (info.RecordsAffected != null)
						sb.Append($" Rows Count: {info.RecordsAffected}.");

					sb.AppendLine();

					logger.LogInformation(_messageCounter, sb.ToString());

					break;
				}
			}
		}

		public virtual ILogger CreateLogger(IDbContextOptions options)
		{
			var coreOptions = options?.FindExtension<CoreOptionsExtension>();

			var logger = coreOptions?.LoggerFactory?.CreateLogger("LinqToDB");
			if (logger != null)
			{
				if (DataConnection.TraceSwitch.Level == TraceLevel.Off)
					DataConnection.TurnTraceSwitchOn();
			}

			return logger;
		}

		/// <summary>
		/// Gets or sets default provider version for SQL Server. Set to <see cref="SqlServerVersion.v2008"/> dialect.
		/// </summary>
		public static SqlServerVersion SqlServerDefaultVersion { get; set; } = SqlServerVersion.v2008;

		/// <summary>
		/// Gets or sets default provider version for PostgreSQL Server. Set to <see cref="PostgreSQLVersion.v93"/> dialect.
		/// </summary>
		public static PostgreSQLVersion PostgreSqlDefaultVersion { get; set; } = PostgreSQLVersion.v93;

	}
}
