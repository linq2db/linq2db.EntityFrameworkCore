using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	[TestFixture]
	public class ForMappingTests : ForMappingTestsBase
	{
		public override ForMappingContextBase CreateContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseSqlServer("Server=.;Database=ForMapping;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			var options = optionsBuilder.Options;
			var ctx = new ForMappingContext(options);

			//ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();

			return ctx;
		}
	}
}
