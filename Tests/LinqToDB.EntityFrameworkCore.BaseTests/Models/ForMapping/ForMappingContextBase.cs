using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping
{
	public abstract class ForMappingContextBase : DbContext
	{
		protected ForMappingContextBase(DbContextOptions options) : base(options)
		{
		}

		public DbSet<WithIdentity> WithIdentity => Set<WithIdentity>();
		public DbSet<NoIdentity> NoIdentity => Set<NoIdentity>();
		public DbSet<UIntTable> UIntTable => Set<UIntTable>();
		public DbSet<StringTypes> StringTypes => Set<StringTypes>();
		public DbSet<WithEnums> WithEnums => Set<WithEnums>();

		public DbSet<WithDuplicateProperties> WithDuplicateProperties => Set<WithDuplicateProperties>();
	}
}
