using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Identity
{
	public class IdentityDbContext : DbContext
	{
		public IdentityDbContext(DbContextOptions<DbContext> options) : base(options)
		{
		}

		public DbSet<Person> People { get; set; } = null!;
	}
}
