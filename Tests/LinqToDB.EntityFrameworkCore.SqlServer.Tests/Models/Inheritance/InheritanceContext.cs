using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Inheritance
{
	public abstract class BlogBase
	{
		public int Id { get; set; }
		public string BlogType { get; set; } = null!;
	}

	public class Blog : BlogBase
	{
		public string Url { get; set; } = null!;
	}

	public class RssBlog : BlogBase
	{
		public string Url { get; set; } = null!;
	}

	public abstract class ShadowBlogBase
	{
		public int Id { get; set; }
		public string BlogType { get; set; } = null!;
	}

	public class ShadowBlog : ShadowBlogBase
	{
		public string Url { get; set; } = null!;
	}

	public class ShadowRssBlog : ShadowBlogBase
	{
		public string Url { get; set; } = null!;
	}

	public interface IVersionable
	{
		int Id { get; set; }
		int? ParentVersionId { get; set; }
	}

	public class InheritanceContext : DbContext
	{
		public InheritanceContext(DbContextOptions options) : base(options)
		{
		}

		private void VersionEntity()
		{
			ChangeTracker.DetectChanges();
			var modifiedEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Entity is IVersionable);

			foreach (var modifiedEntry in modifiedEntries)
			{
				var cloned = (IVersionable)Activator.CreateInstance(modifiedEntry.Entity.GetType());
				modifiedEntry.CurrentValues.SetValues(cloned);

				// rollback
				modifiedEntry.CurrentValues.SetValues(modifiedEntry.OriginalValues);

				cloned.Id = 0;
				cloned.ParentVersionId = ((IVersionable)(modifiedEntry).Entity).Id;

				Add((object)cloned);
			}
		}
		
		public override int SaveChanges()
		{
			return base.SaveChanges();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<BlogBase>()
				.HasDiscriminator(b => b.BlogType)
				.HasValue<Blog>("blog_base")
				.HasValue<RssBlog>("blog_rss");

			modelBuilder.Entity<BlogBase>()
				.Property(e => e.BlogType)
				.HasColumnName("BlogType")
				.HasMaxLength(200);

			modelBuilder.Entity<Blog>()
				.Property(b => b.Url)
				.HasColumnName("Url");

			modelBuilder.Entity<RssBlog>()
				.Property(b => b.Url)
				.HasColumnName("Url");

			modelBuilder.Entity<Blog>().ToTable("Blogs");
			modelBuilder.Entity<RssBlog>().ToTable("Blogs");

			/////

			modelBuilder.Entity<ShadowBlogBase>()
				.HasDiscriminator()
				.HasValue<ShadowBlog>("blog_base")
				.HasValue<ShadowRssBlog>("blog_rss");

			modelBuilder.Entity<ShadowBlogBase>()
				.Property(e => e.BlogType)
				.HasColumnName("BlogType")
				.HasMaxLength(200);

			modelBuilder.Entity<ShadowBlog>()
				.Property(b => b.Url)
				.HasColumnName("Url");

			modelBuilder.Entity<ShadowRssBlog>()
				.Property(b => b.Url)
				.HasColumnName("Url");

			modelBuilder.Entity<ShadowBlog>().ToTable("ShadowBlogs");
			modelBuilder.Entity<ShadowRssBlog>().ToTable("ShadowBlogs");
		}

		public DbSet<BlogBase> Blogs { get; set; } = null!;
		public DbSet<ShadowBlogBase> ShadowBlogs { get; set; } = null!;
	}

}
