using System;
using System.Linq;
using FluentAssertions;
using LinqToDB.DataProvider.SqlServer;
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

		public override ForMappingContextBase CreateContext(Func<DataOptions, DataOptions>? optionsSetter = null)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseSqlServer(Settings.ForMappingConnectionString);
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			if (optionsSetter! != null)
				optionsBuilder.UseLinqToDB(builder => builder.AddCustomOptions(optionsSetter));

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
				var ms = LinqToDBForEFTools.GetMappingSchema(db.Model, db, null);
				var ed = ms.GetEntityDescriptor(typeof(StringTypes));

				ed.Columns.First(c => c.MemberName == nameof(StringTypes.AnsiString)).DataType.Should()
					.Be(DataType.VarChar);

				ed.Columns.First(c => c.MemberName == nameof(StringTypes.UnicodeString)).DataType.Should()
					.Be(DataType.NVarChar);
			}
		}

		[Test]
		public void TestDialectUse()
		{
			using var db = CreateContext(o => o.UseSqlServer("TODO:remove after fix from linq2db (not used)", SqlServerVersion.v2005));
			using var dc = db.CreateLinqToDBConnectionDetached();
			Assert.True(dc.MappingSchema.DisplayID.Contains("2005"));
		}
	}
}
