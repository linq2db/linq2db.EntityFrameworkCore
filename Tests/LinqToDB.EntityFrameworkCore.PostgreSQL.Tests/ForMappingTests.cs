using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests
{
	[TestFixture]
	public class ForMappingTests : ForMappingTestsBase
	{
		private bool _isDbCreated;

		public override ForMappingContextBase CreateContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseNpgsql("Server=DBHost;Port=5432;Database=ForMapping;User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			var options = optionsBuilder.Options;
			var ctx = new ForMappingContext(options);

			if (!_isDbCreated)
			{
				ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();

				_isDbCreated = true;
			}

			return ctx;
		}

		//Disabled, we cannot create such identity table.
		public override void TestBulkCopyWithIdentity()
		{
		}

	}
}
