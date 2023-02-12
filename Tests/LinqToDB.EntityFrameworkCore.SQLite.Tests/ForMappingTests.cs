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
		public override ForMappingContextBase CreateContext(DataOptions? dataOptions = null)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseSqlite("DataSource=:memory:");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			//if (dataOptions! != null)
			//{
			//	optionsBuilder.UseLinqToDB((_, _) => dataOptions);
			//}
			optionsBuilder.UseLinqToDB((_, options) => dataOptions ?? options);

			var options = optionsBuilder.Options;
			var ctx = new ForMappingContext(options);

			ctx.Database.OpenConnection();
			ctx.Database.EnsureCreated();

			return ctx;
		}
	}
}
