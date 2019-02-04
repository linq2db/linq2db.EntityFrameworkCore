using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using JetBrains.Annotations;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;
	using Linq;

	public static partial class LinqToDBForEFTools
	{
		#region BulkCopy

		/// <summary>
		/// Performs bulk insert operation.
		/// </summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="options">Operation options.</param>
		/// <param name="source">Records to insert.</param>
		/// <returns>Bulk insert operation status.</returns>
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, BulkCopyOptions options, IEnumerable<T> source) where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return dc.BulkCopy(options, source);
			}
		}

		/// <summary>
		/// Performs bulk insert operation.
		/// </summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="maxBatchSize">Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server. </param>
		/// <param name="source">Records to insert.</param>
		/// <returns>Bulk insert operation status.</returns>
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, int maxBatchSize, IEnumerable<T> source) where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return dc.BulkCopy(
					new BulkCopyOptions { MaxBatchSize = maxBatchSize },
					source);
			}
		}

		/// <summary>
		/// Performs bulk insert operation.
		/// </summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="source">Records to insert.</param>
		/// <returns>Bulk insert operation status.</returns>
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, IEnumerable<T> source) where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return dc.BulkCopy(
					new BulkCopyOptions(),
					source);
			}
		}

		#endregion

		#region Value Insertable

		/// <summary>
		/// Starts LINQ query definition for insert operation.
		/// </summary>
		/// <typeparam name="T">Target table mapping class.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="target">Target table.</param>
		/// <returns>Insertable source query.</returns>
		[LinqTunnel]
		[Pure]
		public static IValueInsertable<T> Into<T>([NotNull] this DbContext context, [NotNull] ITable<T> target)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (target == null)  throw new ArgumentNullException(nameof(target));

			return context.CreateLinqToDbConnection().Into(target);
		}

		#endregion

		#region GetTable

		/// <summary>
		/// Returns queryable source for specified mapping class for current DBContext, mapped to database table or view.
		/// </summary>
		/// <typeparam name="T">Mapping class type.</typeparam>
		/// <returns>Queryable source.</returns>
		public static ITable<T> GetTable<T>([NotNull] this DbContext context)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.CreateLinqToDbContext().GetTable<T>();
		}
		
		#endregion
	}
}
