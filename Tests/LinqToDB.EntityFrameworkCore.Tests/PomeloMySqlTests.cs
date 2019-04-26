using System;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.Tests.Models.NpgSqlEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using NUnit.Framework;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	public class PomeloMySqlTests : TestsBase
	{
		private DbContextOptions<NorthwindContext> _options;

		static PomeloMySqlTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public PomeloMySqlTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseMySql(
				"Server=DBHost;Port=3306;Database=test_ef_data;Uid=root;Pwd=TestPassword;charset=utf8;",
				builder => builder.ServerVersion(Version.Parse("4.5.7"), ServerType.MySql));

			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NorthwindContext CreateMySqlSqlExntitiesContext()
		{
			var ctx = new NorthwindContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}


		[Test]
		public void SimpleProviderTest()
		{
			using (var db = CreateMySqlSqlExntitiesContext())
			{
				var items = db.Customers.Where(e => e.Address != null).ToLinqToDB().ToArray();
			}
		}

	
	}
}
