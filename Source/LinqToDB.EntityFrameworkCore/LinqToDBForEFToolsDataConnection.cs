using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinqToDB.EntityFrameworkCore
{
	using System.Diagnostics.CodeAnalysis;
	using Data;
	using DataProvider;
	using Linq;
	using Expressions;
	using LinqToDB.Interceptors;
	using System.Data.Common;

	/// <summary>
	/// linq2db EF.Core data connection.
	/// </summary>
	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor, IEntityServiceInterceptor
	{
		readonly IModel? _model;
		readonly Func<Expression, IDataContext, DbContext?, IModel?, Expression>? _transformFunc;

		private IEntityType?   _lastEntityType;
		private Type?          _lastType;
		private IStateManager? _stateManager;

		private static IMemoryCache _entityKeyGetterCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

		private static MethodInfo TryGetEntryMethodInfo =
			MemberHelper.MethodOf<IStateManager>(sm => sm.TryGetEntry(null!, Array.Empty<object>()));

		/// <summary>
		/// Change tracker enable flag.
		/// </summary>
		public bool      Tracking { get; set; }

		/// <summary>
		/// EF.Core database context.
		/// </summary>
		public DbContext? Context  { get; }

		/// <summary>
		/// Creates new instance of data connection.
		/// </summary>
		/// <param name="context">EF.Core database context.</param>
		/// <param name="dataProvider">linq2db database provider.</param>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="transformFunc">Expression converter.</param>
		public LinqToDBForEFToolsDataConnection(
			DbContext?     context,
			[NotNull]   IDataProvider dataProvider,
			[NotNull]   string        connectionString,
			            IModel?       model,
			Func<Expression, IDataContext, DbContext?, IModel?, Expression>? transformFunc) : base(dataProvider, connectionString)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			CopyDatabaseProperties();
			if (LinqToDBForEFTools.EnableChangeTracker)
				AddInterceptor(this);
		}

		/// <summary>
		/// Creates new instance of data connection.
		/// </summary>
		/// <param name="context">EF.Core database context.</param>
		/// <param name="dataProvider">linq2db database provider.</param>
		/// <param name="transaction">Database transaction.</param>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="transformFunc">Expression converter.</param>
		public LinqToDBForEFToolsDataConnection(
			DbContext?      context,
			[NotNull]   IDataProvider dataProvider,
			[NotNull]   DbTransaction transaction,
			            IModel?       model,
			Func<Expression, IDataContext, DbContext?, IModel?, Expression>? transformFunc)
				: base(dataProvider, transaction)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			CopyDatabaseProperties();
			if (LinqToDBForEFTools.EnableChangeTracker)
				AddInterceptor(this);
		}

		/// <summary>
		/// Creates new instance of data connection.
		/// </summary>
		/// <param name="context">EF.Core database context.</param>
		/// <param name="dataProvider">linq2db database provider.</param>
		/// <param name="connection">Database connection instance.</param>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="transformFunc">Expression converter.</param>
		public LinqToDBForEFToolsDataConnection(
			DbContext?     context,
			[NotNull]   IDataProvider dataProvider,
			[NotNull]   DbConnection  connection,
			            IModel?       model,
			Func<Expression, IDataContext, DbContext?, IModel?, Expression>? transformFunc) : base(dataProvider, connection)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			CopyDatabaseProperties();
			if (LinqToDBForEFTools.EnableChangeTracker)
				AddInterceptor(this);
		}

		/// <summary>
		/// Converts expression using convert function, passed to context.
		/// </summary>
		/// <param name="expression">Expression to convert.</param>
		/// <returns>Converted expression.</returns>
		public Expression ProcessExpression(Expression expression)
		{
			if (_transformFunc == null)
				return expression;
			return _transformFunc(expression, this, Context, _model);
		}

		private sealed class TypeKey
		{
			public TypeKey(IEntityType entityType, IModel? model)
			{
				EntityType = entityType;
				Model = model;
			}

			public IEntityType EntityType { get; }
			public IModel?     Model      { get; }

			private bool Equals(TypeKey other)
			{
				return EntityType.Equals(other.EntityType) && Equals(Model, other.Model);
			}

			public override bool Equals(object? obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != GetType())
				{
					return false;
				}

				return Equals((TypeKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (EntityType.GetHashCode() * 397) ^ (Model != null ? Model.GetHashCode() : 0);
				}
			}
		}

		object IEntityServiceInterceptor.EntityCreated(EntityCreatedEventData eventData, object entity)
		{
			// Do not allow to store in ChangeTracker temporary tables
			if ((eventData.TableOptions & TableOptions.IsTemporaryOptionSet) != 0)
				return entity;

			// Do not allow to store in ChangeTracker tables from different server
			if (eventData.ServerName != null)
				return entity;

			if (!LinqToDBForEFTools.EnableChangeTracker
			    || !Tracking
			    || Context!.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking)
				return entity;

			var type = entity.GetType();
			if (_lastType != type)
			{
				_lastType       = type;
				_lastEntityType = Context.Model.FindEntityType(type);
			}

			if (_lastEntityType == null)
				return entity;

			// Do not allow to store in ChangeTracker tables that has different name
			if (eventData.TableName != _lastEntityType.GetTableName())
				return entity;

			_stateManager ??= Context.GetService<IStateManager>();

			// It is a real pain to register entity in change tracker
			//
			InternalEntityEntry? entry = null;

			var kacheKey = new TypeKey (_lastEntityType, _model);

			var retrievalFunc = _entityKeyGetterCache.GetOrCreate(kacheKey, ce =>
			{
				ce.SlidingExpiration = TimeSpan.FromHours(1);
				return CreateEntityRetrievalFunc(((TypeKey)ce.Key).EntityType);
			});

			if (retrievalFunc == null)
				return entity;

			entry = retrievalFunc(_stateManager, entity);

			entry ??= _stateManager.StartTrackingFromQuery(_lastEntityType, entity, ValueBuffer.Empty);

			return entry.Entity;
		}

		private Func<IStateManager, object, InternalEntityEntry?>? CreateEntityRetrievalFunc(IEntityType entityType)
		{
			var stateManagerParam = Expression.Parameter(typeof(IStateManager), "sm");
			var objParam = Expression.Parameter(typeof(object), "o");

			var variable = Expression.Variable(entityType.ClrType, "e");
			var assignExpr = Expression.Assign(variable, Expression.Convert(objParam, entityType.ClrType));

			var key = entityType.GetKeys().FirstOrDefault();
			if (key == null)
				return null;

			var arrayExpr = key.Properties.Where(p => p.PropertyInfo != null || p.FieldInfo != null).Select(p =>
					Expression.Convert(Expression.MakeMemberAccess(variable, p.PropertyInfo ?? (MemberInfo)p.FieldInfo!),
						typeof(object)))
				.ToArray();

			if (arrayExpr.Length == 0)
				return null;

			var newArrayExpression = Expression.NewArrayInit(typeof(object), arrayExpr);
			var body =
				Expression.Block(new[] { variable },
					assignExpr,
					Expression.Call(stateManagerParam, TryGetEntryMethodInfo, Expression.Constant(key),
						newArrayExpression));

			var lambda =
				Expression.Lambda<Func<IStateManager, object, InternalEntityEntry?>>(body, stateManagerParam, objParam);

			return lambda.Compile();
		}

		private void CopyDatabaseProperties()
		{
			var commandTimeout = Context?.Database.GetCommandTimeout();
			if (commandTimeout != null)
				CommandTimeout = commandTimeout.Value;
		}
	}
}
