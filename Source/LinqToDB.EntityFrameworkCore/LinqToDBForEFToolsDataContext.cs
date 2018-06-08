using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{

	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataContext : DataContext, IExpressionPreprocessor
	{
		private readonly IModel _model;
		private readonly Func<Expression, IDataContext, IModel, Expression> _transformFunc;

		public LinqToDBForEFToolsDataContext(
			[NotNull] IDataProvider dataProvider, 
			[NotNull] string connectionString, 
			IModel    model,
			Func<Expression, IDataContext, IModel, Expression> transformFunc) : base(dataProvider, connectionString)
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
