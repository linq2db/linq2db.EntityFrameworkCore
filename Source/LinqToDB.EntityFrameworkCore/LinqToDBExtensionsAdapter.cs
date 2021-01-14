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
		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AsAsyncEnumerable{TSource}(IQueryable{TSource})"/>
		public IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(IQueryable<TSource> source)
			=> EntityFrameworkQueryableExtensions.AsAsyncEnumerable(source);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ForEachAsync{T}(IQueryable{T}, Action{T}, CancellationToken)"/>
		public Task ForEachAsync<TSource>(
			IQueryable<TSource> source,
			Action<TSource>     action,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.ForEachAsync(source, action, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToListAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<List<TSource>> ToListAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.ToListAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToArrayAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource[]> ToArrayAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.ToArrayAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToDictionaryAsync{TSource, TKey}(IQueryable{TSource}, Func{TSource, TKey}, CancellationToken)"/>
		public Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
			IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			CancellationToken   cancellationToken)
			where TKey : notnull
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToDictionaryAsync{TSource, TKey}(IQueryable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey}, CancellationToken)"/>
		public Task<Dictionary<TKey,TSource>> ToDictionaryAsync<TSource,TKey>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			IEqualityComparer<TKey>  comparer,
			CancellationToken        cancellationToken)
			where TKey : notnull
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, comparer, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToDictionaryAsync{TSource, TKey, TElement}(IQueryable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, CancellationToken)"/>
		public Task<Dictionary<TKey,TElement>> ToDictionaryAsync<TSource,TKey,TElement>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			Func<TSource,TElement>   elementSelector,
			CancellationToken        cancellationToken)
			where TKey : notnull
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ToDictionaryAsync{TSource, TKey, TElement}(IQueryable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey}, CancellationToken)"/>
		public Task<Dictionary<TKey,TElement>> ToDictionaryAsync<TSource,TKey,TElement>(
			IQueryable<TSource>      source,
			Func<TSource,TKey>       keySelector,
			Func<TSource,TElement>   elementSelector,
			IEqualityComparer<TKey>  comparer,
			CancellationToken        cancellationToken)
			where TKey : notnull
			=> EntityFrameworkQueryableExtensions.ToDictionaryAsync(source, keySelector, elementSelector, comparer, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.FirstAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> FirstAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.FirstAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<TSource> FirstAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.FirstAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.FirstOrDefaultAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> FirstOrDefaultAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.FirstOrDefaultAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<TSource> FirstOrDefaultAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SingleAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> SingleAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SingleAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<TSource> SingleAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.SingleAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SingleOrDefaultAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> SingleOrDefaultAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SingleOrDefaultAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<TSource> SingleOrDefaultAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.ContainsAsync{TSource}(IQueryable{TSource}, TSource, CancellationToken)"/>
		public Task<bool> ContainsAsync<TSource>(
			IQueryable<TSource> source,
			TSource             item,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.ContainsAsync(source, item, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AnyAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<bool> AnyAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AnyAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<bool> AnyAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.AnyAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AllAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<bool> AllAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.AllAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.CountAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<int> CountAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.CountAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<int> CountAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.CountAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.LongCountAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<long> LongCountAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.LongCountAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
		public Task<long> LongCountAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,bool>> predicate,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.LongCountAsync(source, predicate, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.MinAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> MinAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.MinAsync{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}}, CancellationToken)"/>
		public Task<TResult> MinAsync<TSource,TResult>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.MinAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.MaxAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		public Task<TSource> MaxAsync<TSource>(
			IQueryable<TSource> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.MaxAsync{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}}, CancellationToken)"/>
		public Task<TResult> MaxAsync<TSource,TResult>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,TResult>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.MaxAsync(source, selector, cancellationToken);

		#region SumAsync

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{int}, CancellationToken)"/>
		public Task<int> SumAsync(
			IQueryable<int>   source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{int?}, CancellationToken)"/>
		public Task<int?> SumAsync(
			IQueryable<int?>  source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{long}, CancellationToken)"/>
		public Task<long> SumAsync(
			IQueryable<long>  source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{long?}, CancellationToken)"/>
		public Task<long?> SumAsync(
			IQueryable<long?> source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{float}, CancellationToken)"/>
		public Task<float> SumAsync(
			IQueryable<float> source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{float?}, CancellationToken)"/>
		public Task<float?> SumAsync(
			IQueryable<float?> source,
			CancellationToken  cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{double}, CancellationToken)"/>
		public Task<double> SumAsync(
			IQueryable<double> source,
			CancellationToken  cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{double?}, CancellationToken)"/>
		public Task<double?> SumAsync(
			IQueryable<double?> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{decimal}, CancellationToken)"/>
		public Task<decimal> SumAsync(
			IQueryable<decimal> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync(IQueryable{decimal?}, CancellationToken)"/>
		public Task<decimal?> SumAsync(
			IQueryable<decimal?> source,
			CancellationToken    cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, int}}, CancellationToken)"/>
		public Task<int> SumAsync<TSource>(
			IQueryable<TSource>           source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, int?}}, CancellationToken)"/>
		public Task<int?> SumAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, long}}, CancellationToken)"/>
		public Task<long> SumAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, long?}}, CancellationToken)"/>
		public Task<long?> SumAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, float}}, CancellationToken)"/>
		public Task<float> SumAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, float?}}, CancellationToken)"/>
		public Task<float?> SumAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, double}}, CancellationToken)"/>
		public Task<double> SumAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, double?}}, CancellationToken)"/>
		public Task<double?> SumAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, decimal}}, CancellationToken)"/>
		public Task<decimal> SumAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.SumAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, decimal?}}, CancellationToken)"/>
		public Task<decimal?> SumAsync<TSource>(
			IQueryable<TSource>                source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  cancellationToken)
			=> EntityFrameworkQueryableExtensions.SumAsync(source, selector, cancellationToken);

		#endregion SumAsync

		#region AverageAsync

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{int}, CancellationToken)"/>
		public Task<double> AverageAsync(
			IQueryable<int>   source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{int?}, CancellationToken)"/>
		public Task<double?> AverageAsync(
			IQueryable<int?>  source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{long}, CancellationToken)"/>
		public Task<double> AverageAsync(
			IQueryable<long>  source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{long?}, CancellationToken)"/>
		public Task<double?> AverageAsync(
			IQueryable<long?> source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{float}, CancellationToken)"/>
		public Task<float> AverageAsync(
			IQueryable<float> source,
			CancellationToken cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{float?}, CancellationToken)"/>
		public Task<float?> AverageAsync(
			IQueryable<float?> source,
			CancellationToken  cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{double}, CancellationToken)"/>
		public Task<double> AverageAsync(
			IQueryable<double> source,
			CancellationToken  cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{double?}, CancellationToken)"/>
		public Task<double?> AverageAsync(
			IQueryable<double?> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{decimal}, CancellationToken)"/>
		public Task<decimal> AverageAsync(
			IQueryable<decimal> source,
			CancellationToken   cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync(IQueryable{decimal?}, CancellationToken)"/>
		public Task<decimal?> AverageAsync(
			IQueryable<decimal?> source,
			CancellationToken    cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, int}}, CancellationToken)"/>
		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>           source,
			Expression<Func<TSource,int>> selector,
			CancellationToken             cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, int?}}, CancellationToken)"/>
		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,int?>> selector,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, long}}, CancellationToken)"/>
		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>            source,
			Expression<Func<TSource,long>> selector,
			CancellationToken              cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, long?}}, CancellationToken)"/>
		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,long?>> selector,
			CancellationToken               cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, float}}, CancellationToken)"/>
		public Task<float> AverageAsync<TSource>(
			IQueryable<TSource>             source,
			Expression<Func<TSource,float>> selector,
			CancellationToken               cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, float?}}, CancellationToken)"/>
		public Task<float?> AverageAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,float?>> selector,
			CancellationToken                cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, double}}, CancellationToken)"/>
		public Task<double> AverageAsync<TSource>(
			IQueryable<TSource>              source,
			Expression<Func<TSource,double>> selector,
			CancellationToken                cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, double?}}, CancellationToken)"/>
		public Task<double?> AverageAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,double?>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, decimal}}, CancellationToken)"/>
		public Task<decimal> AverageAsync<TSource>(
			IQueryable<TSource>               source,
			Expression<Func<TSource,decimal>> selector,
			CancellationToken                 cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		/// <inheritdoc cref="EntityFrameworkQueryableExtensions.AverageAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, decimal?}}, CancellationToken)"/>
		public Task<decimal?> AverageAsync<TSource>(
			IQueryable<TSource>                source,
			Expression<Func<TSource,decimal?>> selector,
			CancellationToken                  cancellationToken)
			=> EntityFrameworkQueryableExtensions.AverageAsync(source, selector, cancellationToken);

		#endregion AverageAsync
	}
}
