using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinqToDB.EntityFrameworkCore
{
	using System.Diagnostics.CodeAnalysis;
	using Data;
	using DataProvider;
	using Linq;

	/// <summary>
	/// linq2db EF.Core data connection.
	/// </summary>
	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor
	{
		readonly IModel? _model;
		readonly Func<Expression, IDataContext, DbContext?, IModel?, Expression>? _transformFunc;

		private IEntityType?   _lastEntityType;
		private Type?          _lastType;
		private IStateManager? _stateManager;

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
				OnEntityCreated += OnEntityCreatedHandler;
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
			[NotNull]   IDataProvider  dataProvider,
			[NotNull]   IDbTransaction transaction,
			            IModel?        model,
			Func<Expression, IDataContext, DbContext?, IModel?, Expression>? transformFunc
			) : base(dataProvider, transaction)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			CopyDatabaseProperties();
			if (LinqToDBForEFTools.EnableChangeTracker)
				OnEntityCreated += OnEntityCreatedHandler;
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
			[NotNull]   IDbConnection connection,
			            IModel?       model,
			Func<Expression, IDataContext, DbContext?, IModel?, Expression>? transformFunc) : base(dataProvider, connection)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			CopyDatabaseProperties();
			if (LinqToDBForEFTools.EnableChangeTracker)
				OnEntityCreated += OnEntityCreatedHandler;
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

		private void OnEntityCreatedHandler(EntityCreatedEventArgs args)
		{
			if (!LinqToDBForEFTools.EnableChangeTracker
			    || !Tracking 
			    || Context!.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking)
				return;

			var type = args.Entity.GetType();
			if (_lastType != type)
			{
				_lastType       = type;
				_lastEntityType = Context.Model.FindEntityType(type);
			}

			if (_lastEntityType == null)
				return;

			if (_stateManager == null)
				_stateManager = Context.GetService<IStateManager>();


			// It is a real pain to register entity in change tracker
			//
			InternalEntityEntry? entry = null;

			foreach (var key in _lastEntityType.GetKeys())
			{
				//TODO: Find faster way
				var keyArray = key.Properties.Where(p => p.PropertyInfo != null || p.FieldInfo != null).Select(p =>
					p.PropertyInfo != null
						? p.PropertyInfo.GetValue(args.Entity)
						: p.FieldInfo.GetValue(args.Entity)).ToArray();

				if (keyArray.Length == key.Properties.Count)
				{
					entry = _stateManager.TryGetEntry(key, keyArray);

					if (entry != null)
						break;
				}
			}

			if (entry == null)
			{
				entry = _stateManager.StartTrackingFromQuery(_lastEntityType, args.Entity, ValueBuffer.Empty);
			}

			args.Entity = entry.Entity;
		}

		private void CopyDatabaseProperties()
		{
			var commandTimeout = Context?.Database.GetCommandTimeout();
			if (commandTimeout != null)
				CommandTimeout = commandTimeout.Value;
		}
	}
}
