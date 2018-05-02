using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;
	using Expressions;
	using Mapping;
	using Metadata;

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
		/// <summary>
		/// Returns LINQ To DB provider, based on provider data from EF.Core.
		/// Could be overriden if you have issues with default detection mechanisms.
		/// </summary>
		/// <param name="providerInfo">Provider information, extracted from EF.Core.</param>
		/// <returns>LINQ TO DB provider instance.</returns>
		public virtual IDataProvider GetDataProvider(EFProviderInfo providerInfo)
		{
			var info = GetLinqToDbProviderInfo(providerInfo);

			return CreateLinqToDbDataProvider(providerInfo, info);
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
				case "IBM.EntityFrameworkCore-lnx":
				case "IBM.EntityFrameworkCore-osx":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.DB2LUW };
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
				case "SqlServerOptionsExtension":
					return new LinqToDBProviderInfo { ProviderName = ProviderName.SqlServer };
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
		/// Creates metadata provider for specified EF.Core data model. Default implementation uses
		/// <see cref="EFCoreMetadataReader"/> metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <returns>LINQ To DB metadata provider for specified EF.Core model.</returns>
		public virtual IMetadataReader CreateMetadataReader(IModel model)
		{
			return new EFCoreMetadataReader(model);
		}

		/// <summary>
		/// Creates mapping schema using provided EF.Core data model and metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="metadataReader">Additional optional LINQ To DB database metadata provider.</param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		public virtual MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader)
		{
			var schema = new MappingSchema();
			if (metadataReader != null)
				schema.AddMetadataReader(metadataReader);
			return schema;
		}

		/// <summary>
		/// Returns EF.Core <see cref="DbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="DbContextOptions"/> instance.</returns>
		public virtual DbContextOptions GetContextOptions(DbContext context)
		{
			if (context == null)
				return null;

			var prop = typeof(DbContext).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
			if (prop == null)
				return null;

			return prop.GetValue(context) as DbContextOptions;
		}

		private static readonly MethodInfo GetTableMethodInfo =
			MemberHelper.MethodOf<IDataContext>(dc => dc.GetTable<object>()).GetGenericMethodDefinition();

		/// <summary>
		/// Transforms EF.Core expression tree to LINQ To DB expression.
		/// Method replaces EF.Core <see cref="EntityQueryable{TResult}"/> instances with LINQ To DB
		/// <see cref="DataExtensions.GetTable{T}(IDataContext)"/> calls.
		/// </summary>
		/// <param name="expression">EF.Core expression tree.</param>
		/// <param name="dataContext">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <returns>Transformed expression.</returns>
		public virtual Expression TransformExpression(Expression expression, IDataContext dataContext)
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
									Expression.Constant(dataContext)
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
				throw new LinqToDBForEFToolsException($"Can not find private field '{compiler.GetType()}._queryContextFactory' in current EFCore Version");

			var queryContextFactory  = (RelationalQueryContextFactory) queryContextFactoryField.GetValue(compiler);	    
			var dependenciesProperty = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new LinqToDBForEFToolsException($"Can not find private property '{nameof(RelationalQueryContextFactory)}.Dependencies' in current EFCore Version");

			var dependencies = (QueryContextDependencies) dependenciesProperty.GetValue(queryContextFactory);

			return dependencies.CurrentDbContext?.Context;
		}

		/// <summary>
		/// Extracts EF.Core connection information object from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core connection data.</returns>
		public virtual EFConnectionInfo ExtractConnectionInfo(DbContextOptions options)
		{
			var relational = options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			return new  EFConnectionInfo
			{
				ConnectionString = relational?.ConnectionString,
				Connection = relational?.Connection
			};
		}

		/// <summary>
		/// Extracts EF.Core data model instance from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core data model instance.</returns>
		public virtual IModel ExtractModel(DbContextOptions options)
		{
			var coreOptions = options.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault();
			return coreOptions?.Model;
		}

		private static int _messageCounter;

		public virtual void LogConnectionTrace(TraceInfo trace, ILogger logger)
		{
			switch (trace.TraceInfoStep)
			{
				case TraceInfoStep.BeforeExecute:
					logger.LogInformation(Interlocked.Increment(ref _messageCounter), trace.SqlText);
					break;
				case TraceInfoStep.AfterExecute:
					break;
				case TraceInfoStep.Error:
					logger.LogError(trace.Exception, "Error during execution");
					break;
				case TraceInfoStep.MapperCreated:
					break;
				case TraceInfoStep.Completed:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public virtual ILogger CreateLogger(DbContextOptions options)
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
		/// Gets or sets default provider version for SQL Server. Set to <see cref="PostgreSQLVersion.v93"/> dialect.
		/// </summary>
		public static PostgreSQLVersion PotgreSqlDefaultVersion { get; set; } = PostgreSQLVersion.v93;

	}
}
