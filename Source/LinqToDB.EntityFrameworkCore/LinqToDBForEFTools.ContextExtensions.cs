using JetBrains.Annotations;
using LinqToDB.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;

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
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, BulkCopyOptions options, IEnumerable<T> source)
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
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, int maxBatchSize, IEnumerable<T> source)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return dc.DataProvider.BulkCopy(
					dc,
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
		public static BulkCopyRowsCopied BulkCopy<T>([NotNull] this DbContext context, IEnumerable<T> source)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return dc.DataProvider.BulkCopy(
					dc,
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
	}
}
