using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{
	/// <summary>
	/// LINQ To DB async extensions adapter to call EF.Core functionality instead of default implementation.
	/// </summary>
	public class LinqToDBExtensionsAdapter : IExtensionsAdapter
	{
		public Task ForEachAsync<TSource>(
			IQueryable<TSource> source,
			Action<TSource>     action,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.ForEachAsync(source, action, token);

		public Task<List<TSource>> ToListAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.ToListAsync(source, token);

		public Task<TSource[]> ToArrayAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.ToArrayAsync(source, token);

		public Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
			IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, token);

		public Task<Dictionary<TKey,TSource>> ToDictionaryAsync<TSource,TKey>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			IEqualityComparer<TKey>  comparer,
			CancellationToken        token)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, comparer, token);

		public Task<Dictionary<TKey,TElement>> ToDictionaryAsync<TSource,TKey,TElement>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			Func<TSource,TElement>   elementSelector,
			CancellationToken        token)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, token);

		public Task<Dictionary<TKey,TElement>> ToDictionaryAsync<TSource,TKey,TElement>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			Func<TSource,TElement>   elementSelector,
			IEqualityComparer<TKey>  comparer,
			CancellationToken        token)
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, comparer, token);

		public Task<TSource> FirstAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, token);

		public Task<TSource> FirstAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, predicate, token);

		public Task<TSource> FirstOrDefaultAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, token);

		public Task<TSource> FirstOrDefaultAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, predicate, token);

		public Task<TSource> SingleAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, token);

		public Task<TSource> SingleAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, predicate, token);

		public Task<TSource> SingleOrDefaultAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, token);

		public Task<TSource> SingleOrDefaultAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, predicate, token);

		public Task<bool> ContainsAsync<TSource>(
			IQueryable<TSource> source,
			TSource             item,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.ContainsAsync(source, item, token);

		public Task<bool> AnyAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, token);

		public Task<bool> AnyAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, predicate, token);

		public Task<bool> AllAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.AllAsync(source, predicate, token);

		public Task<int> CountAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, token);

		public Task<int> CountAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, predicate, token);

		public Task<long> LongCountAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, token);

		public Task<long> LongCountAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, predicate, token);

		public Task<TSource> MinAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, token);

		public Task<TResult> MinAsync<TSource,TResult>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, selector, token);

		public Task<TSource> MaxAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, token);

		public Task<TResult> MaxAsync<TSource,TResult>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, selector, token);

		#region SumAsync

		public Task<int> SumAsync(
			IQueryable<int>   source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<int?> SumAsync(
			IQueryable<int?>  source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<long> SumAsync(
			IQueryable<long>  source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<long?> SumAsync(
			IQueryable<long?> source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<float> SumAsync(
			IQueryable<float> source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<float?> SumAsync(
			IQueryable<float?> source,
			CancellationToken  token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<double> SumAsync(
			IQueryable<double> source,
			CancellationToken  token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<double?> SumAsync(
			IQueryable<double?> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<decimal> SumAsync(
			IQueryable<decimal> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<decimal?> SumAsync(
			IQueryable<decimal?> source,
			CancellationToken    token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, token);

		public Task<int> SumAsync<TSource>(
			IQueryable<TSource>           source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<int?> SumAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<long> SumAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<long?> SumAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<float> SumAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<float?> SumAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<double> SumAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<double?> SumAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<decimal> SumAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		public Task<decimal?> SumAsync<TSource>(
			IQueryable<TSource>                source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, token);

		#endregion SumAsync

		#region AverageAsync

		public Task<double> AverageAsync(
			IQueryable<int>   source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double?> AverageAsync(
			IQueryable<int?>  source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double> AverageAsync(
			IQueryable<long>  source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double?> AverageAsync(
			IQueryable<long?> source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<float> AverageAsync(
			IQueryable<float> source,
			CancellationToken token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<float?> AverageAsync(
			IQueryable<float?> source,
			CancellationToken  token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double> AverageAsync(
			IQueryable<double> source,
			CancellationToken  token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double?> AverageAsync(
			IQueryable<double?> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<decimal> AverageAsync(
			IQueryable<decimal> source,
			CancellationToken   token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<decimal?> AverageAsync(
			IQueryable<decimal?> source,
			CancellationToken    token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, token);

		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>           source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<float> AverageAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<float?> AverageAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<decimal> AverageAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		public Task<decimal?> AverageAsync<TSource>(
			IQueryable<TSource>                source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  token)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, token);

		#endregion AverageAsync
	}
}
