using System;
using System.Data;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinqToDB.EntityFrameworkCore
{

	using Data;
	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor
	{
		readonly IModel _model;
		readonly Func<Expression, IDataContext, DbContext, IModel, Expression> _transformFunc;

		private IEntityType   _lastEntityType;
		private Type          _lastType;
		private IStateManager _stateManager;

		public bool      Tracking { get; set; }

		public DbContext Context  { get; }

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext     context,
			[NotNull]   IDataProvider dataProvider, 
			[NotNull]   string        connectionString, 
			            IModel        model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc) : base(dataProvider, connectionString)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			if (LinqToDBForEFTools.EnableChangeTracker)
				OnEntityCreated += OnEntityCreatedHandler;
		}

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext      context,
			[NotNull]   IDataProvider  dataProvider, 
			[NotNull]   IDbTransaction transaction,
			            IModel         model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc
			) : base(dataProvider, transaction)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			if (LinqToDBForEFTools.EnableChangeTracker)
				OnEntityCreated += OnEntityCreatedHandler;
		}

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext     context,
			[NotNull]   IDataProvider dataProvider, 
			[NotNull]   IDbConnection connection, 
			            IModel        model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc) : base(dataProvider, connection)
		{
			Context          = context;
			_model           = model;
			_transformFunc   = transformFunc;
			if (LinqToDBForEFTools.EnableChangeTracker)
				OnEntityCreated += OnEntityCreatedHandler;
		}

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
			    || Context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking)
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

			var entry   = _stateManager.StartTrackingFromQuery(_lastEntityType, args.Entity, ValueBuffer.Empty);
			args.Entity = entry.Entity;
		}

	}
}
