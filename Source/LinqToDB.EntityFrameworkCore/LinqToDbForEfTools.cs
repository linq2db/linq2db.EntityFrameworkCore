using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LinqToDB.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;
	using DataProvider;
	using Linq;
	using Mapping;
	using Metadata;

	/// <summary>
	/// EF.Core to LINQ To DB integration.
	/// </summary>
	[PublicAPI]
	public static partial class LinqToDBForEFTools
	{
		private static Lazy<bool> _intialized = new Lazy<bool>(InitializeInternal);

		/// <summary>
		/// Initializes integration of LINQ To DB with EF.Core.
		/// </summary>
		public static void Initialize()
		{
			var _ = _intialized.Value;
		}

		private static bool InitializeInternal()
		{
			var prev = LinqExtensions.ProcessSourceQueryable;

			var instantiator = MemberHelper.MethodOf(() => Internals.CreateExpressionQueryInstance<int>(null, null))
				.GetGenericMethodDefinition();

			LinqExtensions.ProcessSourceQueryable = queryable =>
			{
				// our Provider nothing to do
				if (queryable.Provider is IQueryProviderAsync)
					return queryable;

				var context = Implementation.GetCurrentContext(queryable);
				if (context == null)
					throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

				var dc = CreateLinqToDbConnection(context);
				var newExpression = TransformExpression(queryable.Expression, dc);

				var result = (IQueryable)instantiator.MakeGenericMethod(queryable.ElementType)
					.Invoke(null, new object[] { dc, newExpression });

				if (prev != null)
					result = prev(result);

				return result;
			};

			LinqExtensions.ExtensionsAdapter = new LinqToDBExtensionsAdapter();

			return true;
		}


		private static ILinqToDBForEFTools _implementation;

		/// <summary>
		/// Gets or sets EF.Core to LINQ To DB integration bridge implementation.
		/// </summary>
		public static ILinqToDBForEFTools Implementation
		{
			get => _implementation;
			set
			{
				_implementation = value ?? throw new ArgumentNullException(nameof(value));
				_metadataReaders.Clear();
				_defaultMeadataReader = new Lazy<IMetadataReader>(() => Implementation.CreateMetadataReader(null));
			}
		}

		private static readonly ConcurrentDictionary<IModel, IMetadataReader> _metadataReaders = new ConcurrentDictionary<IModel, IMetadataReader>();

		private static Lazy<IMetadataReader> _defaultMeadataReader;

	    static LinqToDBForEFTools()
	    {
			Implementation = new LinqToDBForEFToolsImplDefault();
			Initialize();
		}

		/// <summary>
		/// Creates or return existing metadata provider for provided EF.Core data model. If model is null, empty metadata
		/// provider will be returned.
		/// </summary>
		/// <param name="model">EF.Core data model instance. Could be <c>null</c>.</param>
		/// <returns>LINQ To DB metadata provider.</returns>
		public static IMetadataReader GetMetadataReader([JetBrains.Annotations.CanBeNull] IModel model)
		{
			if (model == null)
				return _defaultMeadataReader.Value;

			return _metadataReaders.GetOrAdd(model, m => Implementation.CreateMetadataReader(model));
		}

		/// <summary>
		/// Returns EF.Core <see cref="DbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="DbContextOptions"/> instance.</returns>
		public static DbContextOptions GetContextOptions(DbContext context)
		{
			return Implementation.GetContextOptions(context);
		}

		/// <summary>
		/// Returns EF.Core database provider information for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns>EF.Core provider information.</returns>
		public static EFProviderInfo GetEfProviderInfo(DbContext context)
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
		/// Returns EF.Core database provider information for specific <see cref="DbConnection"/> instance.
		/// </summary>
		/// <param name="connection">EF.Core <see cref="DbConnection"/> instance.</param>
		/// <returns>EF.Core provider information.</returns>
		public static EFProviderInfo GetEfProviderInfo(DbConnection connection)
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
		/// Returns EF.Core database provider information for specific <see cref="DbContextOptions"/> instance.
		/// </summary>
		/// <param name="options">EF.Core <see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core provider information.</returns>
		public static EFProviderInfo GetEfProviderInfo(DbContextOptions options)
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
		/// Returns LINQ To DB provider, based on provider data from EF.Core.
		/// </summary>
		/// <param name="info">EF.Core provider information.</param>
		/// <returns>LINQ TO DB provider instance.</returns>
		public static IDataProvider GetDataProvider(EFProviderInfo info)
	    {
		    var provider = Implementation.GetDataProvider(info);

		    if (provider == null)
				throw new LinqToDBForEFToolsException("Can not detect provider from Entity Framework or provider not supported");

			return provider;
		}

		/// <summary>
		/// Creates mapping schema using provided EF.Core data model.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		public static MappingSchema GetMappingSchema(IModel model)
		{
			return Implementation.GetMappingSchema(model, GetMetadataReader(model));
		}

		/// <summary>
		/// Transforms EF.Core expression tree to LINQ To DB expression.
		/// </summary>
		/// <param name="expression">EF.Core expression tree.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <returns>Transformed expression.</returns>
		public static Expression TransformExpression(Expression expression, IDataContext dc)
		{
			return Implementation.TransformExpression(expression, dc);
		}

		/// <summary>
		/// Creates LINQ To DB <see cref="DataConnection"/> instance, attached to provided
		/// EF.Core <see cref="DbContext"/> instance connection and transaction.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <param name="transaction">Optional transaction instance, to which created connection should be attached.
		/// If not specified, will use current <see cref="DbContext"/> transaction if it available.</param>
		/// <returns>LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinqToDbConnection(this DbContext context,
			IDbContextTransaction transaction = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var info = GetEfProviderInfo(context);

			DataConnection dc = null;

			transaction = transaction ?? context.Database.CurrentTransaction;

			var provider = GetDataProvider(info);

			if (transaction != null)
			{
				var dbTrasaction = transaction.GetDbTransaction();
				if (provider.IsCompatibleConnection(dbTrasaction.Connection))
					dc = new DataConnection(provider, dbTrasaction);
			}

		    if (dc == null)
		    {
			    var dbConnection = context.Database.GetDbConnection();
				if (provider.IsCompatibleConnection(dbConnection))
					dc = new DataConnection(provider, context.Database.GetDbConnection());
			    else
				{
					var connectionInfo = GetConnectionInfo(info);
				    dc = new DataConnection(provider, connectionInfo.ConnectionString);
			    }
		    }

		    var mappingSchema = GetMappingSchema(context.Model);
			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
	    }

	    public static IDataContext CreateLinqToDbContext(this DbContext context,
		    IDbContextTransaction transaction = null)
	    {
		    if (context == null) throw new ArgumentNullException(nameof(context));

		    var info = GetEfProviderInfo(context);

		    DataConnection dc = null;

		    transaction = transaction ?? context.Database.CurrentTransaction;

			var provider = GetDataProvider(info);
		    var mappingSchema = GetMappingSchema(context.Model);

			if (transaction != null)
			{
				var dbTrasaction = transaction.GetDbTransaction();
				if (provider.IsCompatibleConnection(dbTrasaction.Connection))
					dc = new DataConnection(provider, dbTrasaction);
			}

		    if (dc == null)
		    {
			    var dbConnection = context.Database.GetDbConnection();
				if (provider.IsCompatibleConnection(dbConnection))
					dc = new DataConnection(provider, context.Database.GetDbConnection());
			    else
				{
					// special case when we have to create data connection by itself
					var connectionInfo = GetConnectionInfo(info);
				    var dataContext = new DataContext(provider, connectionInfo.ConnectionString);
					if (mappingSchema != null)
						dataContext.MappingSchema = mappingSchema;
					return dataContext;
				}
		    }

			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
		}

		//  public static DataContext CreateLinqToDbContext(this DbContext context)
		//  {
		//   if (context == null) throw new ArgumentNullException(nameof(context));

		//   var info = GetEfProviderInfo(context);

		//var dc = new DataContext(GetDataProvider(info), context.Database.GetDbConnection());

		//   var mappingSchema = GetMappingSchema(context.Model);
		//if (mappingSchema != null)
		//	dc.AddMappingSchema(mappingSchema);

		//return dc;
		//  }

		/// <summary>
		/// Creates LINQ To DB <see cref="DataConnection"/> instance that creates new database connection using connection
		/// information from EF.Core <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns>LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinq2DbConnectionDetached([JetBrains.Annotations.NotNull] this DbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var info = GetEfProviderInfo(context);
			var connectionInfo = GetConnectionInfo(info);
			var dataProvider = GetDataProvider(info);

			var dc = new DataConnection(dataProvider, connectionInfo.ConnectionString);

			var mappingSchema = GetMappingSchema(GetModel(GetContextOptions(context)));
			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
		}

		/// <summary>
		/// Extracts database connection information from EF.Core provider data.
		/// </summary>
		/// <param name="info">EF.Core database provider data.</param>
		/// <returns>Database connection information.</returns>
		public static EFConnectionInfo GetConnectionInfo(EFProviderInfo info)
		{
			var connection = info.Connection;
			var connectionString = connection?.ConnectionString;

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
		/// Extracts EF.Core data model instance from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core data model instance.</returns>
		public static IModel GetModel(DbContextOptions options)
		{
			return Implementation.ExtractModel(options);
		}

		/// <summary>
		/// Creates new LINQ To DB <see cref="DataConnection"/> instance using connectivity information from
		/// EF.Core <see cref="DbContextOptions"/> instance.
		/// </summary>
		/// <param name="options">EF.Core <see cref="DbContextOptions"/> instance.</param>
		/// <returns>New LINQ To DB <see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateLinqToDbConnection(this DbContextOptions options)
		{
			var info = GetEfProviderInfo(options);

			DataConnection dc = null;

			var connectionInfo = GetConnectionInfo(info);
			var dataProvider = GetDataProvider(info);
			if (connectionInfo.Connection != null)
				dc = new DataConnection(dataProvider, connectionInfo.Connection);
			else if (connectionInfo.ConnectionString != null)
				dc = new DataConnection(dataProvider, connectionInfo.ConnectionString);

			if (dc == null)
				throw new LinqToDBForEFToolsException($"Can not extract connection information from {nameof(DbContextOptions)}");

			var mappingSchema = GetMappingSchema(GetModel(options));
			if (mappingSchema != null)
				dc.AddMappingSchema(mappingSchema);

			return dc;
		}

		/// <summary>
		/// Converts EF.Core's query to LINQ To DB query and attach it to provided LINQ To DB <see cref="IDataContext"/>.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="query">EF.Core query.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> to use with provided query.</param>
		/// <returns>LINQ To DB query, attached to provided <see cref="IDataContext"/>.</returns>
		public static IQueryable<T> ToLinqToDb<T>(this IQueryable<T> query, IDataContext dc)
		{
			var newExpression = TransformExpression(query.Expression, dc);

			return Internals.CreateExpressionQueryInstance<T>(dc, newExpression);
		}

		/// <summary>
		/// Converts EF.Core's query to LINQ To DB query and attach it to current EF.Core connection.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="query">EF.Core query.</param>
		/// <returns>LINQ To DB query, attached to current EF.Core connection.</returns>
		public static IQueryable<T> ToLinqToDb<T>(this IQueryable<T> query)
		{
			var context = Implementation.GetCurrentContext(query);
			if (context == null)
				throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

			var dc = CreateLinqToDbConnection(context);
		    var newExpression = TransformExpression(query.Expression, dc);

			return Internals.CreateExpressionQueryInstance<T>(dc, newExpression);
		}

		/// <summary>
		/// Extracts <see cref="DbContext"/> instance from <see cref="IQueryable"/> object.
		/// </summary>
		/// <param name="query">EF.Core query.</param>
		/// <returns>Current <see cref="DbContext"/> instance.</returns>
		public static DbContext GetCurrentContext(IQueryable query)
		{
			return Implementation.GetCurrentContext(query);
		}
	}
}
