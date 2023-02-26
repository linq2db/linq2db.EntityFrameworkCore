using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.PomeloMySql.Tests
{
	public class PomeloMySqlTests : TestsBase
	{
		private DbContextOptions<NorthwindContext> _options;

		static PomeloMySqlTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public PomeloMySqlTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			//var connectionString = "Server=DBHost;Port=3306;Database=TestData;Uid=TestUser;Pwd=TestPassword;charset=utf8;";
			var connectionString = "Server=localhost;Port=3316;Database=TestData;Uid=root;Pwd=root;charset=utf8;";
			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NorthwindContext CreateMySqlSqlExntitiesContext()
		{
			var ctx = new NorthwindContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}

		[Test]
		public void SimpleProviderTest()
		{
			using (var db = CreateMySqlSqlExntitiesContext())
			{
				var items = db.Customers.Where(e => e.Address != null).ToLinqToDB().ToArray();
			}
		}
	}
}
