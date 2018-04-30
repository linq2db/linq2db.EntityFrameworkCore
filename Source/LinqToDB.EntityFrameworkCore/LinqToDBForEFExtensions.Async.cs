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

	/// <summary>
	/// Resolves extension conflicts between LinqToDB and EF
	/// </summary>
	[PublicAPI]
	public static partial class LinqToDBForEFExtensions
	{
		public static Task ForEachAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source, 
			Action<TSource>          action, 
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.ForEachAsync(source.ToLinqToDB(), action, token);

		public static Task<List<TSource>> ToListAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source, 
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.ToListAsync(source.ToLinqToDB(), token);

		public static Task<TSource[]> ToArrayAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source, 
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.ToArrayAsync(source.ToLinqToDB(), token);

		public static Task<Dictionary<TKey, TSource>> ToDictionaryAsyncLinqToDB<TSource, TKey>(
			this IQueryable<TSource> source,
			Func<TSource, TKey>      keySelector, 
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, token);

		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncLinqToDB<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			CancellationToken             token = default(CancellationToken))
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, elementSelector, token);

		public static Task<Dictionary<TKey,TElement>> ToDictionaryAsyncLinqToDB<TSource,TKey,TElement>(
			this IQueryable<TSource>      source,
			Func<TSource,TKey>            keySelector,
			Func<TSource,TElement>        elementSelector,
			IEqualityComparer<TKey>       comparer,
			CancellationToken             token = default(CancellationToken))
			=> AsyncExtensions.ToDictionaryAsync(source.ToLinqToDB(), keySelector, elementSelector, comparer, token);

		public static Task<TSource> FirstAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.FirstAsync(source.ToLinqToDB(), token);

		public static Task<TSource> FirstAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.FirstAsync(source.ToLinqToDB(), predicate, token);

		public static Task<TSource> FirstOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.FirstOrDefaultAsync(source.ToLinqToDB(), token);

		public static Task<TSource> FirstOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.FirstOrDefaultAsync(source.ToLinqToDB(), predicate, token);

		public static Task<TSource> SingleAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.SingleAsync(source.ToLinqToDB(), token);

		public static Task<TSource> SingleAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.SingleAsync(source.ToLinqToDB(), predicate, token);

		public static Task<TSource> SingleOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.SingleOrDefaultAsync(source.ToLinqToDB(), token);

		public static Task<TSource> SingleOrDefaultAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.SingleOrDefaultAsync(source.ToLinqToDB(), predicate, token);

		public static Task<bool> ContainsAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source, 
			TSource                  item,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.ContainsAsync(source.ToLinqToDB(), item, token);

		public static Task<bool> AnyAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.AnyAsync(source.ToLinqToDB(), token);

		public static Task<bool> AnyAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.AnyAsync(source.ToLinqToDB(), predicate, token);

		public static Task<bool> AllAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.AllAsync(source.ToLinqToDB(), predicate, token);

		public static Task<int> CountAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.CountAsync(source.ToLinqToDB(), token);

		public static Task<int> CountAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.CountAsync(source.ToLinqToDB(), predicate, token);

		public static Task<long> LongCountAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.LongCountAsync(source.ToLinqToDB(), token);

		public static Task<long> LongCountAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source, 
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.LongCountAsync(source.ToLinqToDB(), predicate, token);

		public static Task<TSource> MinAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.MinAsync(source.ToLinqToDB(), token);

		public static Task<TResult> MinAsyncLinqToDB<TSource,TResult>(
			this IQueryable<TSource>         source, 
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.MinAsync(source.ToLinqToDB(), selector, token);

		public static Task<TSource> MaxAsyncLinqToDB<TSource>(
			this IQueryable<TSource> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.MaxAsync(source.ToLinqToDB(), token);

		public static Task<TResult> MaxAsyncLinqToDB<TSource,TResult>(
			this IQueryable<TSource>          source, 
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.MaxAsync(source.ToLinqToDB(), selector, token);

		#region SumAsync

		public static Task<int> SumAsync(
			this IQueryable<int>   source,
			CancellationToken token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<int?> SumAsync(
			this IQueryable<int?> source,
			CancellationToken     token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<long> SumAsync(
			this IQueryable<long> source,
			CancellationToken     token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<long?> SumAsync(
			this IQueryable<long?> source,
			CancellationToken      token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<float> SumAsync(
			this IQueryable<float> source,
			CancellationToken      token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<float?> SumAsync(
			this IQueryable<float?> source,
			CancellationToken       token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<double> SumAsync(
			this IQueryable<double> source,
			CancellationToken       token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<double?> SumAsync(
			this IQueryable<double?> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<decimal> SumAsync(
			this IQueryable<decimal> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<decimal?> SumAsync(
			this IQueryable<decimal?> source,
			CancellationToken         token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), token);

		public static Task<int> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<int?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<long> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<long?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<float> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<float?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal?> SumAsyncLinqToDB<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default(CancellationToken))
			=> AsyncExtensions.SumAsync(source.ToLinqToDB(), selector, token);

		#endregion SumAsync

		#region AverageAsync

		public static Task<double> AverageAsync(
			this IQueryable<int> source,
			CancellationToken    token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<int?> source,
			CancellationToken     token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsync(
			this IQueryable<long> source,
			CancellationToken     token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<long?> source,
			CancellationToken      token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<float> AverageAsync(
			this IQueryable<float> source,
			CancellationToken      token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<float?> AverageAsync(
			this IQueryable<float?> source,
			CancellationToken       token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsync(
			this IQueryable<double> source,
			CancellationToken       token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double?> AverageAsync(
			this IQueryable<double?> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<decimal> AverageAsync(
			this IQueryable<decimal> source,
			CancellationToken        token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<decimal?> AverageAsync(
			this IQueryable<decimal?> source,
			CancellationToken         token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>      source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>       source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<float> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>        source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<float?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>         source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<double?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>          source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		public static Task<decimal?> AverageAsyncLinqToDB<TSource>(
			this IQueryable<TSource>           source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token = default(CancellationToken))
			=> AsyncExtensions.AverageAsync(source.ToLinqToDB(), selector, token);

		#endregion AverageAsync
	}
}
