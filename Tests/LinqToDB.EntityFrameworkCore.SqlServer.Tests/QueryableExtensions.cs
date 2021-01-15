using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	public static class QueryableExtensions
	{
		public static async Task<IEnumerable<T>> FilterExistentAsync<T, TProp>(this ICollection<T> items,
			IQueryable<T> dbQuery, Expression<Func<T, TProp>> prop, CancellationToken cancellationToken = default)
		{
			var propGetter = prop.Compile();
			var ids = items.Select(propGetter).ToList();
			var parameter = prop.Parameters[0];

			var predicate = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(TProp) }, Expression.Constant(ids), prop.Body);
			var predicateLambda = Expression.Lambda(predicate, parameter);

			var filtered = Expression.Call(typeof(Queryable), "Where", new[] {typeof(T)}, dbQuery.Expression,
				predicateLambda);

			var selectExpr = Expression.Call(typeof(Queryable), "Select", new[] {typeof(T), typeof(TProp)}, filtered, prop);
			var selectQuery = dbQuery.Provider.CreateQuery<TProp>(selectExpr);

			var existingIds = await selectQuery.ToListAsync(cancellationToken);

			return items.Where(i => !existingIds.Contains(propGetter(i)));
		}

		public static IIncludableQueryable<TEntity, TProp> IncludeInline<TEntity, TProp, TInlineProp>(
			this IIncludableQueryable<TEntity, TProp> includable, Expression<Func<TEntity, TInlineProp>> inlineProp)
		{
			var path = new List<Expression>();

			var current = includable.Expression;
			while (current.NodeType == ExpressionType.Call)
			{
				var mc = (MethodCallExpression) current;
				if (mc.Method.Name == "Include" || mc.Method.Name == "ThenInclude")
					path.Add(mc.Arguments[1]);
				else
					break;
			}

			return includable;
		}
	}
}
