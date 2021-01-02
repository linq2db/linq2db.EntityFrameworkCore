﻿using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.NpgSqlEntities
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
				entity.Property(e => e.Duration).HasColumnType("tsrange")
			);

			modelBuilder.Entity<EventView>(entity =>
				{
					entity.HasNoKey();
					entity.ToView("EventsView", "views");
				});
		}

		public virtual DbSet<Event> Events { get; set; } = null!;

	}
}
