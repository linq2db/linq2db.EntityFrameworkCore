using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LinqToDB.EntityFrameworkCore
{
	// ReSharper disable InvokeAsExtensionMethod
	[PublicAPI]
	public static partial class LinqToDBForEFExtensions
	{
		/// <summary>
		/// Asynchronously apply provided action to each element in source sequence.
		/// Sequence elements processed sequentially.
		/// </summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="action">Action to apply to each sequence element.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Asynchronous operation completion task.</returns>
		public static Task ForEachAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			Action<TSource>          action,
			CancellationToken        token = default)
			=> AsyncExtensions.ForEachAsync(source.ToLinqToDB(), action, token);

		/// <summary>
		/// Asynchronously loads data from query to a list.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>List with query results.</returns>
		public static Task<List<TSource>> ToListAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.ToListAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads data from query to an array.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Array with query results.</returns>
		public static Task<TSource[]> ToArrayAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.ToArrayAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads data from query to a dictionary.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <typeparam name="TKey">Dictionary key type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="keySelector">Source element key selector.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Dictionary with query results.</returns>
		public static Task<Dictionary<TKey, TSource>> ToDictionaryAsyncLinqToDB<TSource, TKey>(
			this IQueryable<TSource> source,
			Func<TSource, TKey>      keySelector,
			CancellationToken        token = default)
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, token);

		/// <summary>
		/// Asynchronously loads data from query to a dictionary.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <typeparam name="TKey">Dictionary key type.</typeparam>
		/// <typeparam name="TElement">Dictionary element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="keySelector">Source element key selector.</param>
		/// <param name="elementSelector">Dictionary element selector.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Dictionary with query results.</returns>
		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncLinqToDB<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			CancellationToken             token = default)
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, elementSelector, token);

		/// <summary>
		/// Asynchronously loads data from query to a dictionary.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <typeparam name="TKey">Dictionary key type.</typeparam>
		/// <typeparam name="TElement">Dictionary element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="keySelector">Source element key selector.</param>
		/// <param name="elementSelector">Dictionary element selector.</param>
		/// <param name="comparer">Dictionary key comparer.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Dictionary with query results.</returns>
		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncLinqToDB<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			IEqualityComparer<TKey>       comparer,
			CancellationToken             token = default)
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, elementSelector, comparer, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Throws exception, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> FirstAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.FirstAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Throws exception, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> FirstAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.FirstAsync(source.ToLinqToDB(), predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> FirstOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.FirstOrDefaultAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> FirstOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.FirstOrDefaultAsync(source.ToLinqToDB(), predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Throws exception, if query doesn't return exactly one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> SingleAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.SingleAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Throws exception, if query doesn't return exactly one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> SingleAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.SingleAsync(source.ToLinqToDB(), predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// Throws exception, if query returns more than one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> SingleOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.SingleOrDefaultAsync(source.ToLinqToDB(), token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// Throws exception, if query returns more than one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> SingleOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.SingleOrDefaultAsync(source.ToLinqToDB(), predicate, token);

		public static Task<bool> ContainsAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			TSource                  item,
			CancellationToken        token = default)
			=> AsyncExtensions.ContainsAsync(source.ToLinqToDB(), item, token);

		public static Task<bool> AnyAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.AnyAsync(source.ToLinqToDB(), token);

		public static Task<bool> AnyAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.AnyAsync(source.ToLinqToDB(), predicate, token);

		public static Task<bool> AllAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.AllAsync(source.ToLinqToDB(), predicate, token);

		public static Task<int> CountAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.CountAsync(source.ToLinqToDB(), token);

		public static Task<int> CountAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.CountAsync(source.ToLinqToDB(), predicate, token);

		public static Task<long> LongCountAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.LongCountAsync(source.ToLinqToDB(), token);

		public static Task<long> LongCountAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> AsyncExtensions.LongCountAsync(source.ToLinqToDB(), predicate, token);

		public static Task<TSource> MinAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.MinAsync(source.ToLinqToDB(), token);

		public static Task<TResult> MinAsyncLinqToDB<TSource,TResult>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.MinAsync(source.ToLinqToDB(), selector, token);

		public static Task<TSource> MaxAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> AsyncExtensions.MaxAsync(source.ToLinqToDB(), token);

		public static Task<TResult> MaxAsyncLinqToDB<TSource,TResult>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.MaxAsync(source.ToLinqToDB(), selector, token);

		#region SumAsync

		public static Task<int> SumAsync(
			this IQueryable<int>   source,
			CancellationToken token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<int?> SumAsync(
			this IQueryable<int?> source,
			CancellationToken     token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<long> SumAsync(
			this IQueryable<long> source,
			CancellationToken     token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<long?> SumAsync(
			this IQueryable<long?> source,
			CancellationToken      token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<float> SumAsync(
			this IQueryable<float> source,
			CancellationToken      token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<float?> SumAsync(
			this IQueryable<float?> source,
			CancellationToken       token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<double> SumAsync(
			this IQueryable<double> source,
			CancellationToken       token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<double?> SumAsync(
			this IQueryable<double?> source,
			CancellationToken        token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<decimal> SumAsync(
			this IQueryable<decimal> source,
			CancellationToken        token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<decimal?> SumAsync(
			this IQueryable<decimal?> source,
			CancellationToken         token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<int> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<int?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<long> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<long?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<float> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<float?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default)
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		#endregion SumAsync

		#region AverageAsync

		public static Task<double> AverageAsync(
			this IQueryable<int> source,
			CancellationToken    token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<int?> source,
			CancellationToken     token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsync(
			this IQueryable<long> source,
			CancellationToken     token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<long?> source,
			CancellationToken      token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<float> AverageAsync(
			this IQueryable<float> source,
			CancellationToken      token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<float?> AverageAsync(
			this IQueryable<float?> source,
			CancellationToken       token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsync(
			this IQueryable<double> source,
			CancellationToken       token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<double?> source,
			CancellationToken        token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<decimal> AverageAsync(
			this IQueryable<decimal> source,
			CancellationToken        token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<decimal?> AverageAsync(
			this IQueryable<decimal?> source,
			CancellationToken         token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<float> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<float?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default)
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		#endregion AverageAsync
	}
}
