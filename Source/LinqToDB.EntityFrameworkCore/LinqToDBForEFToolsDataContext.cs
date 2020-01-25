using System;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{

	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataContext : DataContext, IExpressionPreprocessor
	{
		readonly IModel _model;
		readonly Func<Expression, IDataContext, IModel, Expression>? _transformFunc;

		public LinqToDBForEFToolsDataContext(
			IDataProvider dataProvider,
			string? connectionString,
			IModel    model,
			Func<Expression, IDataContext, IModel, Expression>? transformFunc) : base(dataProvider, connectionString)
		{
			_model         = model;
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
