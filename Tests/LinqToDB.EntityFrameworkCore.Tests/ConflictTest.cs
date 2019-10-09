using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using LinqToDB.EntityFrameworkCore.Tests;
using LinqToDB.EntityFrameworkCore.Tests.Models.AdventuresWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using NUnit.Framework;

namespace SomeNamespace
{
	[TestFixture]
	public class ConflictTest
	{
		private readonly DbContextOptions _options;
		private DbContextOptions<AdventureWorksContext> _inmemoryOptions;

		static ConflictTest()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public ConflictTest()
		{
			var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=.;Database=AdventureWorks;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;

			optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseInMemoryDatabase("sample");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_inmemoryOptions = optionsBuilder.Options;
		}

		private AdventureWorksContext CreateAdventureWorksContext()
		{
			var ctx = new AdventureWorksContext(_options);
			ctx.Database.EnsureCreated();
			return ctx;
		}

		[Test]
		public void TestToList()
		{
			using (var ctx = CreateAdventureWorksContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var items = ctx.Addresses.AsNoTracking().ToListAsyncEF();
			}
		}

	}
}
