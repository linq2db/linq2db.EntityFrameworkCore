using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.Tests.Models.NpgSqlEntities
{
	public class NpgSqlEnititesContext : DbContext
	{
        public NpgSqlEnititesContext(DbContextOptions options)
            : base(options)
        {
        }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Event>(entity =>
				entity.Property(e => e.Duration).HasColumnType("tstzrange")
			);
		}

        public virtual DbSet<Event> Events { get; set; }

	}
}
