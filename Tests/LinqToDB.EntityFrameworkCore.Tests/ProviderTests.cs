using System.Linq;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	[TestFixture]
	public class ProviderTests
	{
		[Test]
		public void TestMySql()
		{
			var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseMySql("Server=DBHost;Port=3306;Database=AdventureWorks;Uid=root;Pwd=TestPassword;charset=utf8");
			optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) }));

			var connection1 = optionsBuilder.Options.CreateLinqToDbConnection();
			Assert.AreEqual(ProviderName.MySql, connection1.DataProvider.Name);

			using (var context = new AdventureWorksContext(optionsBuilder.Options))
			{
				context.Database.EnsureCreated();

				var connection2 = optionsBuilder.Options.CreateLinqToDbConnection();
				Assert.AreEqual(ProviderName.MySql, connection2.DataProvider.Name);

				//var query = context.Customers.Where(c => c.CompanyName != "A").ToLinqToDb().ToArray();
			}
		}

		[Test]
		public void TestPostgreSql()
		{
			var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseNpgsql("Server=DBHost;Port=5433;Database=TestData;User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;");
			optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) }));

			var connection1 = optionsBuilder.Options.CreateLinqToDbConnection();
			StringAssert.StartsWith(ProviderName.PostgreSQL, connection1.DataProvider.Name);

			using (var context = new AdventureWorksContext(optionsBuilder.Options))
			{
				var connection2 = optionsBuilder.Options.CreateLinqToDbConnection();
			StringAssert.StartsWith(ProviderName.PostgreSQL, connection2.DataProvider.Name);

				var query = context.Customers.Where(c => c.CompanyName != "A").ToLinqToDb().ToArray();
			}
		}
	}
}
