using System.Reflection;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.IssueModel;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind.Mapping;
using LinqToDB.Expressions;
using LinqToDB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind
{
	public class IssueContext : DbContext
	{
		public DbSet<Issue73Entity> Issue73Entities { get; set; } = null!;

		public IssueContext(DbContextOptions options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Issue73Entity>(b =>
			{
				b.HasKey(x => new { x.Id });

				b.HasOne(x => x.Parent)
					.WithMany(x => x!.Childs)
					.HasForeignKey(x => new { x.ParentId })
					.HasPrincipalKey(x => new { x!.Id });

				b.HasData(new[]
				{
					new Issue73Entity
					{
						Id = 2,
						Name = "Name1_2",
					},
					new Issue73Entity
					{
						Id = 3,
						Name = "Name1_3",
						ParentId = 2
					},
				});
			});
		}
	}
}
