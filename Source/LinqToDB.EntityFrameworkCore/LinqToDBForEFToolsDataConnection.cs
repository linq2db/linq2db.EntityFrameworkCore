using System;
using System.Data;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{

	using Data;
	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor
	{
		readonly DbContext _context;
		readonly IModel _model;
		readonly Func<Expression, IDataContext, DbContext, IModel, Expression> _transformFunc;

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext     context,
			[NotNull]   IDataProvider dataProvider, 
			[NotNull]   string        connectionString, 
			            IModel        model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc) : base(dataProvider, connectionString)
		{
			_context       = context;
			_model         = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext      context,
			[NotNull]   IDataProvider  dataProvider, 
			[NotNull]   IDbTransaction transaction,
			            IModel         model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc
			) : base(dataProvider, transaction)
		{
			_context       = context;
			_model         = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			[CanBeNull] DbContext     context,
			[NotNull]   IDataProvider dataProvider, 
			[NotNull]   IDbConnection connection, 
			            IModel        model,
			Func<Expression, IDataContext, DbContext, IModel, Expression> transformFunc) : base(dataProvider, connection)
		{
			_context       = context;
			_model         = model;
			_transformFunc = transformFunc;
		}

		public Expression ProcessExpression(Expression expression)
		{
			if (_transformFunc == null)
				return expression;
			return _transformFunc(expression, this, _context, _model);
		}
	}
}
