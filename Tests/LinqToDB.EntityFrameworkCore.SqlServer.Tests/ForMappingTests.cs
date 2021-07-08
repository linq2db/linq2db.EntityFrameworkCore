using System.Linq;
using FluentAssertions;
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
		private bool _isDbCreated;

		public override ForMappingContextBase CreateContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseSqlServer("Server=.;Database=ForMapping;Integrated Security=SSPI");
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

		[Test]
		public void TestStringMappings()
		{
			using (var db = CreateContext())
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(db.Model, db);
				var ed = ms.GetEntityDescriptor(typeof(StringTypes));

				ed.Columns.First(c => c.MemberName == nameof(StringTypes.AnsiString)).DataType.Should()
					.Be(DataType.VarChar);

				ed.Columns.First(c => c.MemberName == nameof(StringTypes.UnicodeString)).DataType.Should()
					.Be(DataType.NVarChar);
			}

		}
	}
}
