using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{
	using Internal;
	using Interceptors;

	/// <summary>
	/// LinqToDB context options builder
	/// </summary>
	public class LinqToDBContextOptionsBuilder 
	{
		private readonly LinqToDBOptionsExtension _extension;

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="optionsBuilder"></param>
		public LinqToDBContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
		{
			_extension = optionsBuilder.Options.FindExtension<LinqToDBOptionsExtension>();
		}

		/// <summary>
		/// Registers LinqToDb interceptor
		/// </summary>
		/// <param name="interceptor">The interceptor instance to register</param>
		/// <returns></returns>
		public LinqToDBContextOptionsBuilder AddInterceptor(IInterceptor interceptor)
		{
			_extension.Interceptors.Add(interceptor);
			return this;
		}

		/// <summary>
		/// Make the Linq2Db use EF Core registered interceptors
		/// as long as they also implement Linq2Db interfaces
		/// </summary>
		/// <returns></returns>
		public LinqToDBContextOptionsBuilder UseEfCoreRegisteredInterceptorsIfPossible(bool useEfCoreInterceptors = true)
		{
			_extension.TryToUseEfCoreInterceptors = useEfCoreInterceptors;
			return this;
		}
	}
}
