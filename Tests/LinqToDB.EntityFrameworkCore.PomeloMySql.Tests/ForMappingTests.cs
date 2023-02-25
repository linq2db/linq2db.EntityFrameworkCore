using System;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using LinqToDB.EntityFrameworkCore.PomeloMySql.Tests.Models.ForMapping;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.PomeloMySql.Tests
{
	public class ForMappingTests : ForMappingTestsBase
	{
		private bool _isDbCreated;

		public override ForMappingContextBase CreateContext(Func<DataOptions, DataOptions>? optionsSetter = null)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ForMappingContext>();
			//var connectionString = "Server=DBHost;Port=3306;Database=TestData;Uid=TestUser;Pwd=TestPassword;charset=utf8;";
			var connectionString = "Server=localhost;Port=3316;Database=TestData;Uid=root;Pwd=root;charset=utf8;";
			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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
	}
}
