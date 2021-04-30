using System;
using System.Threading.Tasks;

#pragma warning disable 8604
#pragma warning disable CS8625

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public class Unit
	{

	}
	
	public static class ExceptionExtensions
	{
		public static Unit Throw(this Exception e) => throw e;
	}
	
	public static class AAA
	{
		public static ArrangeResult<T, Unit> Arrange<T>(this T @object, Action<T> action)
		{
			action(@object);
			return new ArrangeResult<T, Unit>(@object, default);
		}

		public static ArrangeResult<T, Unit> Arrange<T>(T @object) 
			=> new(@object, default);

		public static ArrangeResult<T, TMock> Arrange<T, TMock>(this TMock mock, Func<TMock, T> @object)
			where TMock: notnull
			=> new(@object(mock), mock);

		public static ActResult<T, TMock> Act<T, TMock>(this ArrangeResult<T, TMock> arrange, Action<T> act)
			where T : notnull
			where TMock : notnull
		{
			try
			{
				act(arrange.Object);
				return new ActResult<T, TMock>(arrange.Object, arrange.Mock, default);
			}
			catch (Exception e)
			{
				return new ActResult<T, TMock>(arrange.Object, arrange.Mock, e);
			}
		}

		public static ActResult<TResult, TMock> Act<T, TMock, TResult>(this ArrangeResult<T, TMock> arrange, Func<T, TResult> act)
			where TResult : notnull
			where TMock : notnull
		{
			try
			{
				return new ActResult<TResult, TMock>(act(arrange.Object), arrange.Mock, default);
			}
			catch (Exception e)
			{
				return new ActResult<TResult, TMock>(default, arrange.Mock, e);
			}
		}

		public static void Assert<T, TMock>(this ActResult<T, TMock> act, Action<T> assert)
			where T : notnull
			where TMock : notnull
		{
			act.Exception?.Throw();
			assert(act.Object);
		}

		public static void Assert<T, TMock>(this ActResult<T, TMock> act, Action<T, TMock> assert)
			where T : notnull
			where TMock : notnull
		{
			act.Exception?.Throw();
			assert(act.Object, act.Mock);
		}

		public static Task<ArrangeResult<T, Unit>> ArrangeAsync<T>(T @object)
			=> Task.FromResult(new ArrangeResult<T, Unit>(@object, default));

		public static async Task<ActResult<TResult, TMock>> Act<T, TMock, TResult>(this Task<ArrangeResult<T, TMock>> arrange, Func<T, Task<TResult>> act)
			where TMock : notnull
			where TResult : notnull
		{
			var a = await arrange;
			try
			{
				return new ActResult<TResult, TMock>(await act(a.Object), a.Mock, default);
			}
			catch (Exception e)
			{
				return new ActResult<TResult, TMock>(default, a.Mock, e);
			}
		}

		public static async Task Assert<T, TMock>(this Task<ActResult<T, TMock>> act, Func<T, Task> assert)
			where T : notnull
			where TMock : notnull
		{
			var result = await act;
			await assert(result.Object);
		}

		public readonly struct ArrangeResult<T, TMock>
			where TMock : notnull
		{
			internal ArrangeResult(T @object, TMock mock) => (Object, Mock) = (@object, mock);
			internal T Object { get; }
			internal TMock Mock { get; }
		}

		public readonly struct ActResult<T, TMock>
			where T: notnull
		{
			internal ActResult(T @object, TMock mock, Exception? exception)
				=> (Object, Mock, Exception) = (@object, mock, exception);
			internal T Object { get; }
			internal TMock Mock { get; }
			internal Exception? Exception { get; }
		}
	}
}
