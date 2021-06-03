using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using LinqToDB.EntityFrameworkCore.PomeloMySql.Tests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.PomeloMySql.Tests
{
	public class ForMappingTests : ForMappingTestsBase
	{
		public override ForMappingContextBase CreateContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			optionsBuilder.UseMySql("Server=DBHost;Port=3306;Database=TestData;Uid=TestUser;Pwd=TestPassword;charset=utf8;");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			var options = optionsBuilder.Options;
			var ctx = new ForMappingContext(options);

			//ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();

			return ctx;
		}

	}
}
