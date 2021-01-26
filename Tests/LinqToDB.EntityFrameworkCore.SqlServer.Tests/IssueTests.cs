using System.Linq;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	[TestFixture]
	public class IssueTests : TestsBase
	{
		private DbContextOptions<IssueContext> _options;
		private bool _created;

		public IssueTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<IssueContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=.;Database=IssuesEFCore;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private IssueContext CreateContext()
		{
			var ctx = new IssueContext(_options);

			if (!_created)
			{
				//ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();
				_created = true;
			}
			return ctx;
		}


		[Test]
		public void Issue73Test()
		{
			using var ctx = CreateContext();

			var q = ctx.Issue73Entities
				.Where(x => x.Name == "Name1_3")
				.Select(x => x.Parent!.Name + ">" + x.Name);

			var efItems = q.ToList();
			var linq2dbItems = q.ToLinqToDB().ToList();

			AreEqual(efItems, linq2dbItems);
		}

	}
}
