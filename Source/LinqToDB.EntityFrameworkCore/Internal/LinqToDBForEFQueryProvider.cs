using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	extern alias interactive_async;
	extern alias bcl_async;
	using Async;
	using Linq;

	/// <summary>
	///     Adapter for <see cref="IAsyncQueryProvider" />
	///		This is internal API and is not intended for use by Linq To DB applications.
	///		It may change or be removed without further notice.
	/// </summary>
	/// <typeparam name="T">Type of query element.</typeparam>
	public class LinqToDBForEFQueryProvider<T> : IAsyncQueryProvider, IQueryProviderAsync, IQueryable<T>, interactive_async::System.Collections.Generic.IAsyncEnumerable<T>
	{
		/// <summary>
		/// Creates instance of adapter.
		/// </summary>
		/// <param name="dataContext">Data context instance.</param>
		/// <param name="expression">Query expression.</param>
		public LinqToDBForEFQueryProvider(IDataContext dataContext, Expression expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));
			var dataContext1 = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
			QueryProvider = (IQueryProviderAsync) Internals.CreateExpressionQueryInstance<T>(dataContext1, expression);
			QueryProviderAsQueryable = (IQueryable<T>) QueryProvider;
		}

		IQueryProviderAsync QueryProvider { get; }
		IQueryable<T> QueryProviderAsQueryable { get; }

		/// <summary>
		/// Creates <see cref="IQueryable"/> instance from query expression.
		/// </summary>
		/// <param name="expression">Query expression.</param>
		/// <returns><see cref="IQueryable"/> instance.</returns>
		public IQueryable CreateQuery(Expression expression)
		{
			return QueryProvider.CreateQuery(expression);
		}

		/// <summary>
		/// Creates <see cref="IQueryable{T}"/> instance from query expression.
		/// </summary>
		/// <typeparam name="TElement">Query element type.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <returns><see cref="IQueryable{T}"/> instance.</returns>
		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return QueryProvider.CreateQuery<TElement>(expression);
		}

		/// <summary>
		/// Executes query expression.
		/// </summary>
		/// <param name="expression">Query expression.</param>
		/// <returns>Query result.</returns>
		public object? Execute(Expression expression)
		{
			return QueryProvider.Execute(expression);
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <returns>Query result.</returns>
		public TResult Execute<TResult>(Expression expression)
		{
			return QueryProvider.Execute<TResult>(expression);
		}

		/// <summary>
		/// Executes query expression and returns result as <see cref="bcl_async::System.Collections.Generic.IAsyncEnumerable{T}"/> value.
		/// </summary>
		/// <typeparam name="TResult">Type of result element.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result as <see cref="bcl_async::System.Collections.Generic.IAsyncEnumerable{T}"/>.</returns>
		public Task<bcl_async::System.Collections.Generic.IAsyncEnumerable<TResult>> ExecuteAsyncEnumerable<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return QueryProvider.ExecuteAsyncEnumerable<TResult>(expression, cancellationToken);
		}

		class AsyncEnumerableWrapper<TResult>: interactive_async::System.Collections.Generic.IAsyncEnumerable<TResult>
		{
			public class EnumeratorWrapper : interactive_async::System.Collections.Generic.IAsyncEnumerator<TResult>
			{
				private readonly bcl_async::System.Collections.Generic.IAsyncEnumerator<TResult> _enumerator;

				public EnumeratorWrapper(bcl_async::System.Collections.Generic.IAsyncEnumerator<TResult> enumerator)
				{
					_enumerator = enumerator;
				}

				public void Dispose()
				{
					Task.Run(() =>_enumerator.DisposeAsync()).Wait();
				}

				public Task<bool> MoveNext(CancellationToken cancellationToken)
				{
					return _enumerator.MoveNextAsync().AsTask();
				}

				public TResult Current => _enumerator.Current;
			}

			private readonly bcl_async::System.Collections.Generic.IAsyncEnumerable<TResult> _enumerable;

			public AsyncEnumerableWrapper(bcl_async::System.Collections.Generic.IAsyncEnumerable<TResult> enumerable)
			{
				_enumerable = enumerable;
			}

			public interactive_async::System.Collections.Generic.IAsyncEnumerator<TResult> GetEnumerator()
			{
				return new EnumeratorWrapper(_enumerable.GetAsyncEnumerator());
			}
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <returns>Query result.</returns>
		public interactive_async::System.Collections.Generic.IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
		{
			var enumerable = Task.Run(() =>
				QueryProvider.ExecuteAsyncEnumerable<TResult>(expression, CancellationToken.None)).Result;

			return new AsyncEnumerableWrapper<TResult>(enumerable);
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result.</returns>
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

		/// <summary>
		/// Type of query element.
		/// </summary>
		public Type ElementType => typeof(T);

		/// <summary>
		/// Query expression.
		/// </summary>
		public Expression Expression => QueryProviderAsQueryable.Expression;

		/// <summary>
		/// Query provider.
		/// </summary>
		public IQueryProvider Provider => this;

		#endregion

		/// <summary>
		/// Gets <see cref="bcl_async::System.Collections.Generic.IAsyncEnumerable{T}"/> for current query.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result as <see cref="bcl_async::System.Collections.Generic.IAsyncEnumerable{T}"/>.</returns>
		public bcl_async::System.Collections.Generic.IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			return Task.Run(() => QueryProvider.ExecuteAsyncEnumerable<T>(Expression, cancellationToken)).Result
				.GetAsyncEnumerator(cancellationToken);
		}

		/// <summary>
		///     Gets an asynchronous enumerator over the sequence.
		/// </summary>
		/// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
		public interactive_async::System.Collections.Generic.IAsyncEnumerator<T> GetEnumerator()
		{
			return new AsyncEnumerableWrapper<T>.EnumeratorWrapper(GetAsyncEnumerator(CancellationToken.None));
		}

		/// <summary>
		/// Returns generated SQL for specific LINQ query.
		/// </summary>
		/// <returns>Generated SQL.</returns>
		public override string? ToString()
		{
			return QueryProvider.ToString();
		}
	}
}
