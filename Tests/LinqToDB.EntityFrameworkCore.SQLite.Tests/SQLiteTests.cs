using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Northwind;
using Microsoft.EntityFrameworkCore;

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
	
	}
}
