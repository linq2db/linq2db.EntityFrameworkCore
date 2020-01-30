using System;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata;

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{

	using DataProvider;
	using Linq;

	public class LinqToDBForEFToolsDataContext : DataContext, IExpressionPreprocessor
	{
		[CanBeNull] 
		readonly DbContext _context;
		readonly IModel _model;
		readonly Func<Expression, IDataContext, DbContext, IModel, Expression> _transformFunc;

		public LinqToDBForEFToolsDataContext(
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

		public Expression ProcessExpression(Expression expression)
		{
			if (_transformFunc == null)
				return expression;
			return _transformFunc(expression, this, _context, _model);
		}

	}
}
