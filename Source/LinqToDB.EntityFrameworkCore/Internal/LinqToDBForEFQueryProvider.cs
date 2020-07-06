using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Query.Internal;

using JetBrains.Annotations;
using LinqToDB.Expressions;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	using Async;
	using Linq;

	/// <summary>
	///     Adapter for <see cref="IAsyncQueryProvider" />
	///		This is internal API and is not intended for use by Linq To DB applications.
	///		It may change or be removed without further notice.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LinqToDBForEFQueryProvider<T> : IAsyncQueryProvider, IQueryProviderAsync, IQueryable<T>, System.Collections.Generic.IAsyncEnumerable<T>
	{
		public LinqToDBForEFQueryProvider([NotNull] IDataContext dataContext, [NotNull] Expression expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));
			var dataContext1 = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
			QueryProvider = (IQueryProviderAsync) Internals.CreateExpressionQueryInstance<T>(dataContext1, expression);
			QueryProviderAsQueryable = (IQueryable<T>) QueryProvider;
		}

		IQueryProviderAsync QueryProvider { get; }
		IQueryable<T> QueryProviderAsQueryable { get; }

		public IQueryable CreateQuery(Expression expression)
		{
			return QueryProvider.CreateQuery(expression);
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return QueryProvider.CreateQuery<TElement>(expression);
		}

		public object Execute(Expression expression)
		{
			return QueryProvider.Execute(expression);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return QueryProvider.Execute<TResult>(expression);
		}

		private static MethodInfo _executeAsyncMethodInfo =
			MemberHelper.MethodOf((IQueryProviderAsync p) => p.ExecuteAsync<int>(null, default)).GetGenericMethodDefinition();

		TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			var item = typeof(TResult).GetGenericArguments()[0];
			var method = _executeAsyncMethodInfo.MakeGenericMethod(item);
			return (TResult) method.Invoke(QueryProvider, new object[] { expression, cancellationToken });
		}

		public System.Collections.Generic.IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
		{
			return QueryProvider.ExecuteAsync<TResult>(expression);
		}

		IAsyncEnumerable<TResult> IQueryProviderAsync.ExecuteAsync<TResult>(Expression expression)
		{
			return QueryProvider.ExecuteAsync<TResult>(expression);
		}

		public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return QueryProvider.ExecuteAsync<TResult>(expression, cancellationToken);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return QueryProviderAsQueryable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return QueryProviderAsQueryable.GetEnumerator();
		}

		#region IQueryable

		public Type ElementType => typeof(T);
		public Expression Expression => QueryProviderAsQueryable.Expression;
		public IQueryProvider Provider => this;

		#endregion

		System.Collections.Generic.IAsyncEnumerator<T> System.Collections.Generic.IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			return ExecuteAsync<T>(Expression).GetAsyncEnumerator(cancellationToken);
		}

	}
}
