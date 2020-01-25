﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{
	// ReSharper disable InvokeAsExtensionMethod
	public static partial class EFForEFExtensions
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
		public static Task ForEachAsyncEF<TSource>(
			this IQueryable<TSource> source,
			Action<TSource>          action,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.ForEachAsync(source, action, token);

		/// <summary>
		/// Asynchronously loads data from query to a list.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>List with query results.</returns>
		public static Task<List<TSource>> ToListAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.ToListAsync(source, token);

		/// <summary>
		/// Asynchronously loads data from query to an array.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Array with query results.</returns>
		public static Task<TSource[]> ToArrayAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.ToArrayAsync(source, token);

		/// <summary>
		/// Asynchronously loads data from query to a dictionary.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <typeparam name="TKey">Dictionary key type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="keySelector">Source element key selector.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>Dictionary with query results.</returns>
		public static Task<Dictionary<TKey, TSource>> ToDictionaryAsyncEF<TSource, TKey>(
			this IQueryable<TSource> source,
			Func<TSource, TKey>      keySelector,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, token);

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
		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncEF<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			CancellationToken             token = default)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, token);

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
		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncEF<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			IEqualityComparer<TKey>       comparer,
			CancellationToken             token = default)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, comparer, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Throws exception, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> FirstAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Throws exception, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> FirstAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> FirstOrDefaultAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> FirstOrDefaultAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Throws exception, if query doesn't return exactly one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> SingleAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, token);

		/// <summary>
		/// Asynchronously loads first record from query, filtered using provided predicate.
		/// Throws exception, if query doesn't return exactly one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="predicate">Query filter predicate.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results.</returns>
		public static Task<TSource> SingleAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, predicate, token);

		/// <summary>
		/// Asynchronously loads first record from query.
		/// Returns <c>default(TSource)</c>, if query doesn't return any records.
		/// Throws exception, if query returns more than one record.
		/// </summary>
		/// <typeparam name="TSource">Query element type.</typeparam>
		/// <param name="source">Source query.</param>
		/// <param name="token">Optional asynchronous operation cancellation token.</param>
		/// <returns>First record from query results or <c>default(TSource)</c> for empty resultset.</returns>
		public static Task<TSource> SingleOrDefaultAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, token);

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
		public static Task<TSource> SingleOrDefaultAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, predicate, token);

		public static Task<bool> ContainsAsyncEF<TSource>(
			this IQueryable<TSource> source,
			TSource                  item,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.ContainsAsync(source, item, token);

		public static Task<bool> AnyAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, token);

		public static Task<bool> AnyAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, predicate, token);

		public static Task<bool> AllAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.AllAsync(source, predicate, token);

		public static Task<int> CountAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, token);

		public static Task<int> CountAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, predicate, token);

		public static Task<long> LongCountAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, token);

		public static Task<long> LongCountAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, predicate, token);

		public static Task<TSource> MinAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, token);

		public static Task<TResult> MinAsyncEF<TSource,TResult>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, selector, token);

		public static Task<TSource> MaxAsyncEF<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, token);

		public static Task<TResult> MaxAsyncEF<TSource,TResult>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, selector, token);

		#region SumAsyncEF

		public static Task<int> SumAsyncEF(
			this IQueryable<int>   source,
			CancellationToken token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<int?> SumAsyncEF(
			this IQueryable<int?> source,
			CancellationToken     token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<long> SumAsyncEF(
			this IQueryable<long> source,
			CancellationToken     token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<long?> SumAsyncEF(
			this IQueryable<long?> source,
			CancellationToken      token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<float> SumAsyncEF(
			this IQueryable<float> source,
			CancellationToken      token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<float?> SumAsyncEF(
			this IQueryable<float?> source,
			CancellationToken       token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<double> SumAsyncEF(
			this IQueryable<double> source,
			CancellationToken       token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<double?> SumAsyncEF(
			this IQueryable<double?> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<decimal> SumAsyncEF(
			this IQueryable<decimal> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<decimal?> SumAsyncEF(
			this IQueryable<decimal?> source,
			CancellationToken         token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public static Task<int> SumAsyncEF<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<int?> SumAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<long> SumAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<long?> SumAsyncEF<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<float> SumAsyncEF<TSource>(
			this IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<float?> SumAsyncEF<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<double> SumAsyncEF<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<double?> SumAsyncEF<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<decimal> SumAsyncEF<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public static Task<decimal?> SumAsyncEF<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		#endregion SumAsyncEF

		#region AverageAsyncEF

		public static Task<double> AverageAsyncEF(
			this IQueryable<int> source,
			CancellationToken    token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double?> AverageAsyncEF(
			this IQueryable<int?> source,
			CancellationToken     token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double> AverageAsyncEF(
			this IQueryable<long> source,
			CancellationToken     token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double?> AverageAsyncEF(
			this IQueryable<long?> source,
			CancellationToken      token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<float> AverageAsyncEF(
			this IQueryable<float> source,
			CancellationToken      token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<float?> AverageAsyncEF(
			this IQueryable<float?> source,
			CancellationToken       token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double> AverageAsyncEF(
			this IQueryable<double> source,
			CancellationToken       token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double?> AverageAsyncEF(
			this IQueryable<double?> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<decimal> AverageAsyncEF(
			this IQueryable<decimal> source,
			CancellationToken        token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<decimal?> AverageAsyncEF(
			this IQueryable<decimal?> source,
			CancellationToken         token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public static Task<double> AverageAsyncEF<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<double?> AverageAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<double> AverageAsyncEF<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<double?> AverageAsyncEF<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<float> AverageAsyncEF<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<float?> AverageAsyncEF<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<double> AverageAsyncEF<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<double?> AverageAsyncEF<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<decimal> AverageAsyncEF<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public static Task<decimal?> AverageAsyncEF<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		#endregion AverageAsyncEF
	}
}
