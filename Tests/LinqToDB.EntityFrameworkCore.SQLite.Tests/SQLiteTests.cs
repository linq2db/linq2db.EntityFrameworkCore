using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Identity;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Northwind;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests
{
	public class SQLiteTests : TestsBase
	{
		private DbContextOptions<NorthwindContext> _options;

		static SQLiteTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public SQLiteTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlite("Data Source=northwind.db;");

			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NorthwindContext CreateSQLiteSqlExntitiesContext()
		{
			var ctx = new NorthwindContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}


		[Test]
		public void TestIdentityMapping()
		{
			using (var ctx = CreateSQLiteSqlExntitiesContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var ed = db.MappingSchema.GetEntityDescriptor(typeof(Category));
				var pk = ed.Columns.Where(c => c.IsPrimaryKey).Single();

				Assert.That(pk.IsIdentity, Is.True);
			}
		}

	
		[Test]
		public void TestSqliteDbCreation()
		{
			var dbFactory = new EfCoreSqliteInMemoryDbFactory();

			using var context = dbFactory.CreateDbContext<IdentityDbContext>();

			context.AddRange(new List<Person>
			{
				new() {Name = "John Doe"},
				new() {Name = "Jane Doe"}
			});

			context.SaveChanges();

			var people = context.People.ToList();

			var connection = context.CreateLinqToDbConnection();

			var tempTable = connection.CreateTempTable(people, new BulkCopyOptions {KeepIdentity = true});

			tempTable.ToList().Should().BeEquivalentTo(people);
		}

	
	}
}
