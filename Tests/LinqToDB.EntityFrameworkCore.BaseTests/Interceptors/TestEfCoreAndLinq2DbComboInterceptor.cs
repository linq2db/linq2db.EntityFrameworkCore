using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Common;
using LinqToDB.Interceptors;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Interceptors
{
	public class TestEfCoreAndLinq2DbComboInterceptor : TestInterceptor, ICommandInterceptor
	{
		#region LinqToDbInterceptor
		public void AfterExecuteReader(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, CommandBehavior commandBehavior, DbDataReader dataReader)
		{
			HasInterceptorBeenInvoked = true;
		}

		public void BeforeReaderDispose(LinqToDB.Interceptors.CommandEventData eventData, DbCommand? command, DbDataReader dataReader)
		{
			HasInterceptorBeenInvoked = true;
		}

		public Task BeforeReaderDisposeAsync(LinqToDB.Interceptors.CommandEventData eventData, DbCommand? command, DbDataReader dataReader)
		{
			HasInterceptorBeenInvoked = true;
			return Task.CompletedTask;
		}

		public DbCommand CommandInitialized(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command)
		{
			HasInterceptorBeenInvoked = true;
			return command;
		}

		public Option<int> ExecuteNonQuery(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, Option<int> result)
		{
			HasInterceptorBeenInvoked = true;
			return result;
		}

		public Task<Option<int>> ExecuteNonQueryAsync(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, Option<int> result, CancellationToken cancellationToken)
		{
			HasInterceptorBeenInvoked = true;
			return Task.FromResult(result);
		}

		public Option<DbDataReader> ExecuteReader(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, CommandBehavior commandBehavior, Option<DbDataReader> result)
		{
			HasInterceptorBeenInvoked = true;
			return result;
		}

		public Task<Option<DbDataReader>> ExecuteReaderAsync(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, CommandBehavior commandBehavior, Option<DbDataReader> result, CancellationToken cancellationToken)
		{
			HasInterceptorBeenInvoked = true;
			return Task.FromResult(result);
		}

		public Option<object?> ExecuteScalar(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, Option<object?> result)
		{
			HasInterceptorBeenInvoked = true;
			return result;
		}

		public Task<Option<object?>> ExecuteScalarAsync(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command, Option<object?> result, CancellationToken cancellationToken)
		{
			HasInterceptorBeenInvoked = true;
			return Task.FromResult(result);
		}
		#endregion
	}
}
