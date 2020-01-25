using System;
using System.Data;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{

	using Data;
	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataConnection : DataConnection, IExpressionPreprocessor
	{
		readonly IModel? _model;
		readonly Func<Expression, IDataContext, IModel?, Expression>? _transformFunc;

		public LinqToDBForEFToolsDataConnection(
			IDataProvider dataProvider,
			string? connectionString,
			IModel? model,
			Func<Expression, IDataContext, IModel?, Expression>? transformFunc) : base(dataProvider, connectionString)
		{
			_model = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			IDataProvider dataProvider,
			IDbTransaction transaction,
			IModel model,
			Func<Expression, IDataContext, IModel?, Expression>? transformFunc) : base(dataProvider, transaction)
		{
			_model = model;
			_transformFunc = transformFunc;
		}

		public LinqToDBForEFToolsDataConnection(
			IDataProvider dataProvider,
			IDbConnection connection,
			IModel? model,
			Func<Expression, IDataContext, IModel?, Expression>? transformFunc) : base(dataProvider, connection)
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
