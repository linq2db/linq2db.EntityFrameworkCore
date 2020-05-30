using System;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.NpgSqlEntities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.Tests
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

			optionsBuilder.UseNpgsql("Server=localhost;Port=5433;Database=test_ef_data;User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NpgSqlEnititesContext CreateNpgSqlExntitiesContext()
		{
			var ctx = new NpgSqlEnititesContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}


		[Test]
		public void TestFunctionsMapping()
		{
			using (var db = CreateNpgSqlExntitiesContext())
			{
				var date = DateTime.UtcNow;

				var query = db.Events.Where(e =>
					e.Duration.Contains(date) || e.Duration.LowerBound == date || e.Duration.UpperBound == date ||
					e.Duration.IsEmpty || e.Duration.Intersect(e.Duration).IsEmpty);

				var efResult = query.ToArray();
				var l2dbResult = query.ToLinqToDB().ToArray();
			}
		}

	
	}
}
