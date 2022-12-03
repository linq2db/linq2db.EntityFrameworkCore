using System.Linq;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests
{
	[TestFixture]
	public class ForMappingTests : ForMappingTestsBase
	{
		public override ForMappingContextBase CreateContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseSqlite("DataSource=:memory:");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			var options = optionsBuilder.Options;
			var ctx = new ForMappingContext(options);

			ctx.Database.OpenConnection();
			ctx.Database.EnsureCreated();

			return ctx;
		}

		[Test]
		public void Issue256Test()
		{
			using var context = CreateContext();

			var record = new WithIdentity()
			{
				Id = -1,
				Name = "initial name"
			};

			var id = context.CreateLinqToDbContext().InsertWithInt32Identity(record);

			var inserted = context.WithIdentity.Where(p => p.Id == id).Single();

			Assert.AreEqual("initial name", inserted.Name);

			var cnt = context.WithIdentity.Where( d => d.Id == id).ToLinqToDB().Set(d => d.Name, "new name").Update();

			Assert.AreEqual(1, cnt);

			var readByLinqToDB = context.WithIdentity.Where(d => d.Id == id).ToLinqToDB().ToArray();

			Assert.AreEqual(1, readByLinqToDB.Length);

			Assert.AreEqual("new name", readByLinqToDB[0].Name);

			var updated = context.WithIdentity.Where(p => p.Id == id).Single();

			Assert.AreEqual("new name", updated.Name);
		}
	}
}
