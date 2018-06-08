using System;
using System.Data;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{

	using Data;
	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor
	{
		private readonly IModel _model;
		private readonly Func<Expression, IDataContext, IModel, Expression> _transformFunc;

		public LinqToDBForEFToolsDataConnection(
			[NotNull] IDataProvider dataProvider, 
			[NotNull] string connectionString, 
			IModel    model,
			Func<Expression, IDataContext, IModel, Expression> transformFunc) : base(dataProvider, connectionString)
		{
			_model = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			[NotNull] IDataProvider dataProvider, 
			[NotNull] IDbTransaction transaction,
			IModel    model,
			Func<Expression, IDataContext, IModel, Expression> transformFunc
			) : base(dataProvider, transaction)
		{
			_model = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			[NotNull] IDataProvider dataProvider, 
			[NotNull] IDbConnection connection, 
			IModel    model,
			Func<Expression, IDataContext, IModel, Expression> transformFunc) : base(dataProvider, connection)
		{
			_model = model;
			_transformFunc = transformFunc;
		}

		public Expression ProcessExpression(Expression expression)
		{
			if (_transformFunc == null)
				return expression;
			return _transformFunc(expression, this, _model);
		}
	}
}
