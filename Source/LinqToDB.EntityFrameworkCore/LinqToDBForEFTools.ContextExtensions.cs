using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using JetBrains.Annotations;

namespace LinqToDB.EntityFrameworkCore
{
	using Data;
	using Linq;
	using Internal;
	using Interceptors;
	using System.Linq;

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
		public static BulkCopyRowsCopied BulkCopy<T>(this DbContext context, BulkCopyOptions options, IEnumerable<T> source) where T : class
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
		public static BulkCopyRowsCopied BulkCopy<T>(this DbContext context, int maxBatchSize, IEnumerable<T> source) where T : class
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
		public static BulkCopyRowsCopied BulkCopy<T>(this DbContext context, IEnumerable<T> source) where T : class
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

		#region BulkCopyAsync

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="options">Operation options.</param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			BulkCopyOptions options,
			IEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(options, source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="maxBatchSize">
		///     Number of rows in each batch. At the end of each batch, the rows in the batch are sent to
		///     the server.
		/// </param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			int maxBatchSize,
			IEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(maxBatchSize, source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			IEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="options">Operation options.</param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			BulkCopyOptions options,
			IAsyncEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(options, source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="maxBatchSize">
		///     Number of rows in each batch. At the end of each batch, the rows in the batch are sent to
		///     the server.
		/// </param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			int maxBatchSize,
			IAsyncEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(maxBatchSize, source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>Asynchronously performs bulk insert operation.</summary>
		/// <typeparam name="T">Mapping type of inserted record.</typeparam>
		/// <param name="context">Database context.</param>
		/// <param name="source">Records to insert.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Task with bulk insert operation status.</returns>
		public static async Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			this DbContext context,
			IAsyncEnumerable<T> source,
			CancellationToken cancellationToken = default)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (source  == null) throw new ArgumentNullException(nameof(source));

			using (var dc = context.CreateLinqToDbConnection())
			{
				return await dc.BulkCopyAsync(source, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
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
		public static IValueInsertable<T> Into<T>(this DbContext context, ITable<T> target)
			where T: notnull
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
		public static ITable<T> GetTable<T>(this DbContext context)
			where T : class
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.CreateLinqToDbContext().GetTable<T>();
		}

		#endregion

		#region Interceptors

		/// <summary>
		/// Returns list of registered Linq2Db interceptors from EF Context
		/// </summary>
		/// <typeparam name="T">Mapping class type.</typeparam>
		/// <returns>Db context object</returns>
		public static IList<IInterceptor>? GetLinq2DbInterceptors(this DbContext context)

		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var contextOptions = ((IInfrastructure<IServiceProvider>)context.Database)?
				.Instance?.GetService(typeof(IDbContextOptions)) as IDbContextOptions;

			return contextOptions?.GetLinq2DbInterceptors();
		}

		/// <summary>
		/// Returns list of registered Linq2Db interceptors from EF Context options
		/// </summary>
		/// <typeparam name="T">Mapping class type.</typeparam>
		/// <returns>Db context options</returns>
		public static IList<IInterceptor> GetLinq2DbInterceptors(this IDbContextOptions contextOptions)
		{
			if (contextOptions == null) throw new ArgumentNullException(nameof(contextOptions));

			var linq2DbExtension = contextOptions?.FindExtension<LinqToDBOptionsExtension>();
			var coreEfExtension = contextOptions?.FindExtension<CoreOptionsExtension>();
			List<IInterceptor> interceptors = new List<IInterceptor>();
			if (coreEfExtension?.Interceptors?.OfType<IInterceptor>().Any() == true)
			{
				interceptors.AddRange(coreEfExtension.Interceptors.OfType<IInterceptor>());
			}

			if (linq2DbExtension?.Interceptors.Any() == true)
			{
				interceptors.AddRange(linq2DbExtension.Interceptors);
			}

			return interceptors;
		}

		#endregion
	}
}
