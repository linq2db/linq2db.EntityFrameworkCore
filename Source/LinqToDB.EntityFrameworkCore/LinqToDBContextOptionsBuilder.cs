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
		private readonly LinqToDBOptionsExtension? _extension;

		/// <summary>
		/// Db context options
		/// </summary>
		public DbContextOptions DbContextOptions { get; private set; }

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="optionsBuilder"></param>
		public LinqToDBContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
		{
			_extension = optionsBuilder.Options.FindExtension<LinqToDBOptionsExtension>();
			DbContextOptions = optionsBuilder.Options;
		}

		/// <summary>
		/// Registers LinqToDb interceptor
		/// </summary>
		/// <param name="interceptor">The interceptor instance to register</param>
		/// <returns></returns>
		public LinqToDBContextOptionsBuilder AddInterceptor(IInterceptor interceptor)
		{
			_extension?.Interceptors.Add(interceptor);
			return this;
		}
	}
}
