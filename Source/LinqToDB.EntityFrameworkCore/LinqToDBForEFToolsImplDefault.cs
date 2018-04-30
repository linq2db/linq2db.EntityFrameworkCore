using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using LinqToDB.DataProvider.SqlCe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace LinqToDB.EntityFrameworkCore
{
	using Expressions;
	using DataProvider;
	using DataProvider.SqlServer;
	using Mapping;
	using Metadata;

	using DataProvider.DB2;
	using DataProvider.Firebird;
	using DataProvider.MySql;
	using DataProvider.Oracle;
	using DataProvider.PostgreSQL;
	using DataProvider.SQLite;

	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	[PublicAPI]
	public partial class LinqToDBForEFToolsImplDefault : ILinqToDBForEFTools
	{
		/// <summary>
		/// Detects Linq2Db provider based on EintityFramework information. 
		/// Should be overriden if you have experienced problem in detecting specific provider. 
		/// </summary>
		/// <param name="providerInfo"></param>
		/// <returns></returns>
		public virtual IDataProvider GetDataProvider(EFProviderInfo providerInfo)
		{
			var info = GetLinqToDbProviderInfo(providerInfo);

			return CreateLinqToDbDataProvider(providerInfo, info);
		}

		protected virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(EFProviderInfo providerInfo)
		{
			LinqToDBProviderInfo provInfo = new LinqToDBProviderInfo();

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

		protected virtual IDataProvider CreateLinqToDbDataProvider(EFProviderInfo providerInfo, LinqToDBProviderInfo provInfo)
		{
			if (provInfo.ProviderName == null)
			{
				throw new LinqToDBForEFToolsException("Can not detect data provider.");
			}

			switch (provInfo.ProviderName)
			{
					case ProviderName.SqlServer:
						return CreateSqlServerProvider(SqlServerDefaultVersion);
					case ProviderName.MySql:
						return new MySqlDataProvider();
					case ProviderName.PostgreSQL:
						return CreatePotgreSqlProvider(PotgreSqlDefaultVersion);
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
					case ProviderName.OracleManaged:
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
				case "MySql.Data.EntityFrameworkCore":
				case "Devart.Data.MySql.EFCore":
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
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2 };
				case "IBM.EntityFrameworkCore-lnx":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2LUW };
				case "IBM.EntityFrameworkCore-osx":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2zOS };
				case "Devart.Data.Oracle.EFCore":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.OracleManaged };
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
						return new LinqToDBProviderInfo { ProviderName = ProviderName.PostgreSQL };
			}

			return null;
		}

		protected  virtual LinqToDBProviderInfo GetLinqToDbProviderInfo(RelationalOptionsExtension extensions)
		{
			switch (extensions.GetType().Name)
			{
				case "MySqlOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.MySql };
				case "NpgsqlOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.PostgreSQL };
			}

			return null;
		}

		protected virtual IDataProvider CreateSqlServerProvider(SqlServerVersion version)
		{
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

		protected virtual IDataProvider CreatePotgreSqlProvider(PostgreSQLVersion version)
		{
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
		/// Creates IMetadataReader implementation. Can be overriden to specify own MetadaData reader
		/// </summary>
		/// <param name="model"></param>
		/// <returns>IMetadataReader implemantetion. Can be null.</returns>
		public virtual IMetadataReader CreateMetadataReader(IModel model)
		{
			return new EFCoreMetadataReader(model);
		}

		/// <summary>
		/// Default implemntation of creation mapping schema for model.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="metadataReader"></param>
		/// <returns>Mapping schema for Model</returns>
		public virtual MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader)
		{
			var schema = new MappingSchema();
			if (metadataReader != null)
				schema.AddMetadataReader(metadataReader);
			return schema;
		}

		/// <summary>
		/// Default implementation of retrieving options from DbContext
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual DbContextOptions GetContextOptions(DbContext context)
		{
			return null;
		}

		public static readonly MethodInfo GetTableMethodInfo =
			MemberHelper.MethodOf<IDataContext>(dc => dc.GetTable<object>()).GetGenericMethodDefinition();


		/// <summary>
		/// Default realisation for IQueryable expression transformation
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="dc"></param>
		/// <returns>Transformed expression</returns>
		public virtual Expression TransformExpression(Expression expression, IDataContext dc)
		{
			var newExpression =
				expression.Transform(e =>
				{
					switch (e.NodeType)
					{
						case ExpressionType.Constant:
						{
							if (LinqToDB.Extensions.ReflectionExtensions.IsSameOrParentOf(typeof(EntityQueryable<>), e.Type))
							{
								var newExpr = Expression.Call(null,
									GetTableMethodInfo.MakeGenericMethod(e.Type.GenericTypeArguments),
									Expression.Constant(dc)
								);
								return newExpr;
							}

							break;
						}
					}

					return e;
				});

			return newExpression;
		}

		public virtual DbContext GetCurrentContext(IQueryable query)
		{
			var compilerField = typeof (EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);
			var compiler = (QueryCompiler) compilerField.GetValue(query.Provider);

			var queryContextFactoryField = compiler.GetType().GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new LinqToDBForEFToolsException($"Can not find private field '{compiler.GetType()}._queryContextFactory' in current EFCore Version");

			var queryContextFactory  = (RelationalQueryContextFactory) queryContextFactoryField.GetValue(compiler);	    
			var dependenciesProperty = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new LinqToDBForEFToolsException($"Can not find private property '{nameof(RelationalQueryContextFactory)}.Dependencies' in current EFCore Version");

			var dependencies = (QueryContextDependencies) dependenciesProperty.GetValue(queryContextFactory);

			return dependencies.CurrentDbContext?.Context;
		}

		public virtual EFConnectionInfo ExtractConnectionInfo(DbContextOptions options)
		{
			var relational = options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			return new  EFConnectionInfo
			{
				ConnectionString = relational?.ConnectionString,
				Connection = relational?.Connection
			};
		}

		public virtual IModel ExtractModel(DbContextOptions options)
		{
			var coreOptions = options.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault();
			return coreOptions?.Model;
		}

		/// <summary>
		/// Default version for SQL Server
		/// </summary>
		public static SqlServerVersion SqlServerDefaultVersion { get; set; } = SqlServerVersion.v2008;

		/// <summary>
		/// Default version for PostgreSQL server
		/// </summary>
		public static PostgreSQLVersion PotgreSqlDefaultVersion { get; set; } = PostgreSQLVersion.v93;

	}
}
