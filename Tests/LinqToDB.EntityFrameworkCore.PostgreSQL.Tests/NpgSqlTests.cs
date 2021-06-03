using System;
using System.Linq;
using FluentAssertions;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.NpgSqlEntities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests
{
	public class NpgSqlTests : TestsBase
	{
		private DbContextOptions<NpgSqlEnititesContext> _options;

		static NpgSqlTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public NpgSqlTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NpgSqlEnititesContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseNpgsql("Server=DBHost;Port=5432;Database=TestData;User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NpgSqlEnititesContext CreateNpgSqlEntitiesContext()
		{
			var ctx = new NpgSqlEnititesContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			ctx.Database.ExecuteSqlRaw("create schema \"views\"");
			ctx.Database.ExecuteSqlRaw("create view \"views\".\"EventsView\" as select \"Name\" from \"Events\"");
			return ctx;
		}

		[Test]
		public void TestFunctionsMapping()
		{
			using (var db = CreateNpgSqlEntitiesContext())
			{
				var date = DateTime.Now;

				var query = db.Events.Where(e =>
					e.Duration.Contains(date) || e.Duration.LowerBound == date || e.Duration.UpperBound == date ||
					e.Duration.IsEmpty || e.Duration.Intersect(e.Duration).IsEmpty);

				var efResult = query.ToArray();
				var l2dbResult = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public void TestViewMapping()
		{
			using (var db = CreateNpgSqlEntitiesContext())
			{
				var query = db.Set<EventView>().Where(e =>
					e.Name.StartsWith("any"));

				var efResult = query.ToArray();
				var l2dbResult = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public void TestArray()
		{
			using (var db = CreateNpgSqlEntitiesContext())
			{
				var guids = new Guid[] { Guid.Parse("271425b1-ebe8-400d-b71d-a6e47a460ae3"),
					Guid.Parse("b75de94e-6d7b-4c70-bfa1-f8639a6a5b35") };

				var query = 
						from m in db.EntityWithArrays.ToLinqToDBTable()
						where Sql.Ext.PostgreSQL().Overlaps(m.Guids, guids)
						select m;

				query.Invoking(q => q.ToArray()).Should().NotThrow();
			}
		}

	}
}
