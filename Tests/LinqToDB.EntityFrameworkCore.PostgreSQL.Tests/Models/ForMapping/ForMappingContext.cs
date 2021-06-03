using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.ForMapping
{
	public class ForMappingContext : ForMappingContextBase
	{

		public ForMappingContext(DbContextOptions options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<WithIdentity>(b =>
			{
				b.HasKey(e => e.Id);

				b.Property(e => e.Id)
					.UseIdentityAlwaysColumn();
			});

			modelBuilder.Entity<NoIdentity>(b =>
			{
				b.HasKey(e => e.Id);
			});
		}
	}
}
