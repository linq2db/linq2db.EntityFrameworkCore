using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping
{
	public abstract class ForMappingContextBase : DbContext
	{
		protected ForMappingContextBase(DbContextOptions options) : base(options)
		{
		}

		public DbSet<WithIdentity> WithIdentity { get; set; } = null!;
		public DbSet<NoIdentity> NoIdentity { get; set; } = null!;

	}
}
