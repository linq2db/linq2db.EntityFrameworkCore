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

		private NorthwindContext CreateMySqlSqlEntitiesContext()
		{
			var ctx = new NorthwindContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}

		[Test]
		public void SimpleProviderTest()
		{
			using (var db = CreateMySqlSqlEntitiesContext())
			{
				var items = db.Customers.Where(e => e.Address != null).ToLinqToDB().ToArray();
			}
		}

		[Test(Description = "https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1801")]
		public void TestFunctionTranslation()
		{
			using var db = CreateMySqlSqlEntitiesContext();
			var items = db.Customers.Where(e => e.Address!.Contains("anything")).ToLinqToDB().ToArray();
		}

		[Test(Description = "https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1801")]
		public void TestFunctionTranslationParameter()
		{
			using var db = CreateMySqlSqlEntitiesContext();
			var value = "anything";
			var items = db.Customers.Where(e => e.Address!.Contains(value)).ToLinqToDB().ToArray();
		}
	}
}
