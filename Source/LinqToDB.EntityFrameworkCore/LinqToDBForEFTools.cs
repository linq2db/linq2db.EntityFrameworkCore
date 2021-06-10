using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LinqToDB.EntityFrameworkCore
{
	using Async;
	using Data;
	using DataProvider;
	using Linq;
	using Mapping;
	using Metadata;
	using Expressions;

	using Internal;

	/// <summary>
	/// EF Core <see cref="DbContext"/> extensions to call LINQ To DB functionality.
	/// </summary>
	[PublicAPI]
	public static partial class LinqToDBForEFTools
	{
		static readonly Lazy<bool> _intialized = new(InitializeInternal);

		/// <summary>
		/// Initializes integration of LINQ To DB with EF Core.
		/// </summary>
		public static void Initialize()
		{
			var _ = _intialized.Value;
		}

		static bool InitializeInternal()
		{
			var prev = LinqExtensions.ProcessSourceQueryable;

			InitializeMapping();

			var instantiator = MemberHelper.MethodOf(() => Internals.CreateExpressionQueryInstance<int>(null!, null!))
				.GetGenericMethodDefinition();

			LinqExtensions.ProcessSourceQueryable = queryable =>
			{
				// our Provider - nothing to do
				if (queryable.Provider is IQueryProviderAsync)
					return queryable;

				var context = Implementation.GetCurrentContext(queryable);
				if (context == null)
					throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

				var dc = CreateLinqToDbContext(context);
				var newExpression = queryable.Expression;

				var result = (IQueryable)instantiator.MakeGenericMethod(queryable.ElementType)
					.Invoke(null, new object[] { dc, newExpression });

				if (prev != null)
					result = prev(result);

				return result;
			};

			LinqExtensions.ExtensionsAdapter = new LinqToDBExtensionsAdapter();

			return true;
		}

		static ILinqToDBForEFTools _implementation = null!;

		/// <summary>
		/// Gets or sets EF Core to LINQ To DB integration bridge implementation.
		/// </summary>
		public static ILinqToDBForEFTools Implementation
		{
			get => _implementation;
			set
			{
				_implementation = value ?? throw new ArgumentNullException(nameof(value));
				_metadataReaders.Clear();
				_defaultMetadataReader = new Lazy<IMetadataReader?>(() => Implementation.CreateMetadataReader(null, null));
			}
		}

		static readonly ConcurrentDictionary<IModel, IMetadataReader?> _metadataReaders = new();

		static Lazy<IMetadataReader?> _defaultMetadataReader = null!;

		/// <summary>
		/// Clears internal caches
		/// </summary>
		public static void ClearCaches()
		{
			_metadataReaders.Clear();
			Implementation.ClearCaches();
			Query.ClearCaches();
		}

		static LinqToDBForEFTools()
		{
			Implementation = new LinqToDBForEFToolsImplDefault();
			Initialize();
		}

		/// <summary>
		/// Creates or return existing metadata provider for provided EF Core data model. If model is null, empty metadata
		/// provider will be returned.
		/// </summary>
		/// <param name="model">EF Core data model instance. Could be <c>null</c>.</param>
		/// <param name="accessor">EF Core service provider.</param>
		/// <returns>LINQ To DB metadata provider.</returns>
		public static IMetadataReader? GetMetadataReader(
			IModel? model,
			IInfrastructure<IServiceProvider>? accessor)
		{
			if (model == null)
				return _defaultMetadataReader.Value;

			return _metadataReaders.GetOrAdd(model, m => Implementation.CreateMetadataReader(model, accessor));
		}

		/// <summary>
		/// Returns EF Core <see cref="DbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="DbContextOptions"/> instance.</returns>
		public static IDbContextOptions? GetContextOptions(DbContext context)
		{
			return Implementation.GetContextOptions(context);
		}

		/// <summary>
		/// Returns EF Core database provider information for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF Core <see cref="DbContext"/> instance.</param>
		/// <returns>EF Core provider information.</returns>
		public static EFProviderInfo GetEFProviderInfo(DbContext context)
		{
			var info = new EFProviderInfo
			{
				Connection = context.Database.GetDbConnection(),
				Context = context,
				Options = GetContextOptions(context)
			};

			return info;
		}

		/// <summary>
		/// Returns EF Core database provider information for specific <see cref="DbConnection"/> instance.
		/// </summary>
		/// <param name="connection">EF Core <see cref="DbConnection"/> instance.</param>
		/// <returns>EF Core provider information.</returns>
		public static EFProviderInfo GetEFProviderInfo(DbConnection connection)
		{
			var info = new EFProviderInfo
			{
				Connection = connection,
				Context = null,
				Options = null
			};

			return info;
		}

		/// <summary>
		/// Returns EF Core database provider information for specific <see cref="DbContextOptions"/> instance.
		/// </summary>
		/// <param name="options">EF Core <see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF Core provider information.</returns>
		public static EFProviderInfo GetEFProviderInfo(DbContextOptions options)
		{
			var info = new EFProviderInfo
			{
				Connection = null,
				Context = null,
				Options = options
			};

			return info;
		}

		/// <summary>
		/// Returns LINQ To DB provider, based on provider data from EF Core.
		/// </summary>
		/// <param name="info">EF Core provider information.</param>
		/// <param name="connectionInfo">Database connection information.</param>
		/// <returns>LINQ TO DB provider instance.</returns>
		public static IDataProvider GetDataProvider(EFProviderInfo info, EFConnectionInfo connectionInfo)
		{
			var provider = Implementation.GetDataProvider(info, connectionInfo);

			if (provider == null)
				throw new LinqToDBForEFToolsException("Can not detect provider from Entity Framework or provider not supported");

			return provider;
		}

		/// <summary>
		/// Creates mapping schema using provided EF Core data model.
		/// </summary>
		/// <param name="model">EF Core data model.</param>
		/// <param name="accessor">EF Core service provider.</param>
		/// <returns>Mapping schema for provided EF Core model.</returns>
		public static MappingSchema GetMappingSchema(
			IModel model,
			IInfrastructure<IServiceProvider>? accessor)
		{
			var converterSelector = accessor?.GetService<IValueConverterSelector>();

			return Implementation.GetMappingSchema(model, GetMetadataReader(model, accessor), converterSelector);
		}

		/// <summary>
		/// Transforms EF Core expression tree to LINQ To DB expression.
		/// </summary>
		/// <param name="expression">EF Core expression tree.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <param name="ctx">Optional DbContext instance.</param>
		/// <param name="model">EF Core data model instance.</param>
		/// <returns>Transformed expression.</returns>
		public static Expression TransformExpression(Expression expression, IDataContext dc, DbContext? ctx, IModel? model)
		{
			return Implementation.TransformExpression(expression, dc, ctx, model);
		}

		/// <summary>
		/// Creates LINQ To DB <see cref="DataConnection"/> instance, attached to provided
		/// EF Core <see cref="DbContext"/> instance connection and transaction.
		/// </summary>
		/// <param name="context">EF Core <see cref="DbContext"/> instance.</param>
		/// <param name="transaction">Optional transaction instance, to which created connection should be attached.
		/// If not specified, will use current <see cref="DbContext"/> transaction if it available.</param>
		/// <returns>LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinqToDbConnection(this DbContext context,
			IDbContextTransaction? transaction = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var info = GetEFProviderInfo(context);

			DataConnection? dc = null;

			transaction = transaction ?? context.Database.CurrentTransaction;

			var connectionInfo = GetConnectionInfo(info);
			var provider = GetDataProvider(info, connectionInfo);

			if (transaction != null)
			{
				var dbTrasaction = transaction.GetDbTransaction();
				// TODO: we need API for testing current connection
				//if (provider.IsCompatibleConnection(dbTrasaction.Connection))
					dc = new LinqToDBForEFToolsDataConnection(context, provider, dbTrasaction, context.Model, TransformExpression);
			}

			if (dc == null)
			{
				var dbConnection = context.Database.GetDbConnection();
				// TODO: we need API for testing current connection
				if (true /*provider.IsCompatibleConnection(dbConnection)*/)
					dc = new LinqToDBForEFToolsDataConnection(context, provider, dbConnection, context.Model, TransformExpression);
				else
				{
					//dc = new LinqToDBForEFToolsDataConnection(context, provider, connectionInfo.ConnectionString, context.Model, TransformExpression);
				}
			}

			var logger = CreateLogger(info.Options);
			if (logger != null)
			{
				EnableTracing(dc, logger);
			}

			var mappingSchema = GetMappingSchema(context.Model, context);
			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
		}

		private static readonly TraceSwitch _defaultTraceSwitch =
			new("DataConnection", "DataConnection trace switch", TraceLevel.Info.ToString());

		static void EnableTracing(DataConnection dc, ILogger logger)
		{
			dc.OnTraceConnection = t => Implementation.LogConnectionTrace(t, logger);
			dc.TraceSwitchConnection = _defaultTraceSwitch;
		}

		/// <summary>
		/// Creates logger intance.
		/// </summary>
		/// <param name="options"><see cref="DbContext" /> options.</param>
		/// <returns>Logger instance.</returns>
		public static ILogger? CreateLogger(IDbContextOptions? options)
		{
			return Implementation.CreateLogger(options);
		}

		/// <summary>
		/// Creates linq2db data context for EF Core database context.
		/// </summary>
		/// <param name="context">EF Core database context.</param>
		/// <param name="transaction">Transaction instance.</param>
		/// <returns>linq2db data context.</returns>
		public static IDataContext CreateLinqToDbContext(this DbContext context,
			IDbContextTransaction? transaction = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var info = GetEFProviderInfo(context);

			DataConnection? dc = null;

			transaction = transaction ?? context.Database.CurrentTransaction;

			var connectionInfo     = GetConnectionInfo(info);
			var provider           = GetDataProvider(info, connectionInfo);
			var mappingSchema      = GetMappingSchema(context.Model, context);
			var logger             = CreateLogger(info.Options);

			if (transaction != null)
			{
				var dbTransaction = transaction.GetDbTransaction();

				// TODO: we need API for testing current connection
				// if (provider.IsCompatibleConnection(dbTransaction.Connection))
				dc = new LinqToDBForEFToolsDataConnection(context, provider, dbTransaction, context.Model, TransformExpression);
			}

			if (dc == null)
			{
				var dbConnection = context.Database.GetDbConnection();
				// TODO: we need API for testing current connection
				if (true /*provider.IsCompatibleConnection(dbConnection)*/)
					dc = new LinqToDBForEFToolsDataConnection(context, provider, context.Database.GetDbConnection(), context.Model, TransformExpression);
				else
				{
					/*
					// special case when we have to create data connection by itself
					var dataContext = new LinqToDBForEFToolsDataContext(context, provider, connectionInfo.ConnectionString, context.Model, TransformExpression);

					if (mappingSchema != null)
						dataContext.MappingSchema = mappingSchema;
					
					if (logger != null)
						dataContext.OnTraceConnection = t => Implementation.LogConnectionTrace(t, logger);
						
					return dataContext;
					*/
				}
			}

			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			if (logger != null)
			{
				EnableTracing(dc, logger);
			}

			return dc;
		}

		/// <summary>
		/// Creates LINQ To DB <see cref="DataConnection"/> instance that creates new database connection using connection
		/// information from EF Core <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF Core <see cref="DbContext"/> instance.</param>
		/// <returns>LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinq2DbConnectionDetached(this DbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var info           = GetEFProviderInfo(context);
			var connectionInfo = GetConnectionInfo(info);
			var dataProvider   = GetDataProvider(info, connectionInfo);

			var dc = new LinqToDBForEFToolsDataConnection(context, dataProvider, connectionInfo.ConnectionString!, context.Model, TransformExpression);
			var logger = CreateLogger(info.Options);

			if (logger != null)
			{
				EnableTracing(dc, logger);
			}

			var mappingSchema = GetMappingSchema(context.Model, context);
			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
		}


		static readonly ConcurrentDictionary<Type, Func<DbConnection, string>> _connectionStringExtractors = new();

		/// <summary>
		/// Extracts database connection information from EF Core provider data.
		/// </summary>
		/// <param name="info">EF Core database provider data.</param>
		/// <returns>Database connection information.</returns>
		public static EFConnectionInfo GetConnectionInfo(EFProviderInfo info)
		{
			var connection = info.Connection;
			string? connectionString = null;
			if (connection != null)
			{
				var connectionStringFunc = _connectionStringExtractors.GetOrAdd(connection.GetType(), t =>
				{
					// NpgSQL workaround
					var originalProp = t.GetProperty("OriginalConnectionString", BindingFlags.Instance | BindingFlags.NonPublic);
					                                     
					if (originalProp == null)
						return c => c.ConnectionString;

					var parameter = Expression.Parameter(typeof(DbConnection), "c");
					var lambda = Expression.Lambda<Func<DbConnection, string>>(
						Expression.MakeMemberAccess(Expression.Convert(parameter, t), originalProp), parameter);

					return lambda.Compile();
				});

				connectionString = connectionStringFunc(connection);
			}

			if (connection != null && connectionString != null)
				return new EFConnectionInfo { Connection = connection, ConnectionString = connectionString };

			var extracted = Implementation.ExtractConnectionInfo(info.Options);

			return new EFConnectionInfo
			{
				Connection = connection ?? extracted?.Connection,
				ConnectionString = extracted?.ConnectionString
			};
		}

		/// <summary>
		/// Extracts EF Core data model instance from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF Core data model instance.</returns>
		public static IModel? GetModel(DbContextOptions? options)
		{
			if (options == null)
				return null;
			return Implementation.ExtractModel(options);
		}

		/// <summary>
		/// Creates new LINQ To DB <see cref="DataConnection"/> instance using connectivity information from
		/// EF Core <see cref="DbContextOptions"/> instance.
		/// </summary>
		/// <param name="options">EF Core <see cref="DbContextOptions"/> instance.</param>
		/// <returns>New LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinqToDbConnection(this DbContextOptions options)
		{
			var info = GetEFProviderInfo(options);

			DataConnection? dc = null;

			var connectionInfo = GetConnectionInfo(info);
			var dataProvider   = GetDataProvider(info, connectionInfo);
			var model          = GetModel(options);

			if (connectionInfo.Connection != null)
				dc = new LinqToDBForEFToolsDataConnection(null, dataProvider, connectionInfo.Connection, model, TransformExpression);
			else if (connectionInfo.ConnectionString != null)
				dc = new LinqToDBForEFToolsDataConnection(null, dataProvider, connectionInfo.ConnectionString, model, TransformExpression);

			if (dc == null)
				throw new LinqToDBForEFToolsException($"Can not extract connection information from {nameof(DbContextOptions)}");

			var logger = CreateLogger(info.Options);
			if (logger != null)
			{
				EnableTracing(dc, logger);
			}

			if (model != null)
			{
				var mappingSchema = GetMappingSchema(model, null);
				if (mappingSchema != null)
					dc.AddMappingSchema(mappingSchema);
			}

			return dc;
		}

		/// <summary>
		/// Converts EF Core's query to LINQ To DB query and attach it to provided LINQ To DB <see cref="IDataContext"/>.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="query">EF Core query.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> to use with provided query.</param>
		/// <returns>LINQ To DB query, attached to provided <see cref="IDataContext"/>.</returns>
		public static IQueryable<T> ToLinqToDB<T>(this IQueryable<T> query, IDataContext dc)
		{
			var context = Implementation.GetCurrentContext(query);
			if (context == null)
				throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

			return new LinqToDBForEFQueryProvider<T>(dc, query.Expression);
		}

		/// <summary>
		/// Converts EF Core's query to LINQ To DB query and attach it to current EF Core connection.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="query">EF Core query.</param>
		/// <returns>LINQ To DB query, attached to current EF Core connection.</returns>
		public static IQueryable<T> ToLinqToDB<T>(this IQueryable<T> query)
		{
			if (query.Provider is IQueryProviderAsync)
			{
				return query;
			}

			var context = Implementation.GetCurrentContext(query);
			if (context == null)
				throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

			var dc = CreateLinqToDbContext(context);

			return new LinqToDBForEFQueryProvider<T>(dc, query.Expression);
		}

		/// <summary>
		/// Extracts <see cref="DbContext"/> instance from <see cref="IQueryable"/> object.
		/// </summary>
		/// <param name="query">EF Core query.</param>
		/// <returns>Current <see cref="DbContext"/> instance.</returns>
		public static DbContext? GetCurrentContext(IQueryable query)
		{
			return Implementation.GetCurrentContext(query);
		}

		/// <summary>
		/// Enables attaching entities to change tracker.
		/// Entities will be attached only if AsNoTracking() is not used in query and DbContext is configured to track entities. 
		/// </summary>
		public static bool EnableChangeTracker 
		{ 
			get => Implementation.EnableChangeTracker;
			set => Implementation.EnableChangeTracker = value;
		}

	}
}
