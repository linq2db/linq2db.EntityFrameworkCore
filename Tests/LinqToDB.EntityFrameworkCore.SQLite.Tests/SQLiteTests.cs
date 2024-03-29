﻿using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Northwind;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests
{
	public class SQLiteTests : TestsBase
	{
		private DbContextOptions<NorthwindContext> _options;

		static SQLiteTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public SQLiteTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();

			optionsBuilder.UseSqlite("Data Source=northwind.db;");

			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		private NorthwindContext CreateSQLiteSqlEntitiesContext()
		{
			var ctx = new NorthwindContext(_options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			return ctx;
		}

		[Test(Description = "https://github.com/linq2db/linq2db.EntityFrameworkCore/issues/343")]
		public void TestFunctionsMapping()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite("Data Source=northwind.db;");
			optionsBuilder.UseLinqToDB(x => x.AddCustomOptions(o => o.UseSQLiteMicrosoft()));
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			using var ctx = new NorthwindContext(optionsBuilder.Options);
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();

			ctx.Categories.ToLinqToDB().ToList();
		}
	}
}
