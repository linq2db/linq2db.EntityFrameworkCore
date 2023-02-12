using System.Data.Common;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Interceptors;
using LinqToDB.EntityFrameworkCore.BaseTests.Interceptors.Extensions;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Northwind;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests
{
	[TestFixture]
	public class InterceptorTests
	{
		private const string SQLITE_CONNECTION_STRING = "DataSource=NorthwindInMemory;Mode=Memory;Cache=Shared";
		private readonly DbContextOptions _northwindOptions;
		private readonly DbContextOptions _northwindOptionsWithEfCoreInterceptorsOnly;
		private DbConnection? _dbConnection;
		static TestCommandInterceptor testCommandInterceptor;
		static TestDataContextInterceptor testDataContextInterceptor;
		static TestConnectionInterceptor testConnectionInterceptor;
		static TestEntityServiceInterceptor testEntityServiceInterceptor;
		static TestEfCoreAndLinq2DbComboInterceptor testEfCoreAndLinq2DbInterceptor;

		static InterceptorTests()
		{
			testCommandInterceptor = new TestCommandInterceptor();
			testDataContextInterceptor = new TestDataContextInterceptor();
			testConnectionInterceptor = new TestConnectionInterceptor();
			testEntityServiceInterceptor = new TestEntityServiceInterceptor();
			testEfCoreAndLinq2DbInterceptor = new TestEfCoreAndLinq2DbComboInterceptor();
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		static DbContextOptions CreateNorthwindOptions()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.UseLinqToDB((builder, options) =>
			{
				return options
					.UseInterceptor(testCommandInterceptor)
					.UseInterceptor(testDataContextInterceptor)
					.UseInterceptor(testConnectionInterceptor)
					.UseInterceptor(testEntityServiceInterceptor)
					.UseInterceptor(testEfCoreAndLinq2DbInterceptor)
					.UseInterceptor(testCommandInterceptor); //for checking the aggregated interceptors
			});
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			return optionsBuilder.Options;
		}

		static DbContextOptions CreateNorthwindOptionsWithEfCoreInterceptorsOnly()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.AddInterceptors(testEfCoreAndLinq2DbInterceptor);
			optionsBuilder.UseLinqToDB((builder, options) =>
			{
				return builder.UseEfCoreRegisteredInterceptorsIfPossible(options);
			});
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			return optionsBuilder.Options;
		}

		public InterceptorTests()
		{
			_northwindOptions = CreateNorthwindOptions();
			_northwindOptionsWithEfCoreInterceptorsOnly = CreateNorthwindOptionsWithEfCoreInterceptorsOnly();
		}

		private NorthwindContext CreateContext()
		{
			var ctx = new NorthwindContext(_northwindOptions);
			return ctx;
		}

		private NorthwindContext CreateContextWithountLinq2DbExtensions()
		{
			var ctx = new NorthwindContext(_northwindOptionsWithEfCoreInterceptorsOnly);
			return ctx;
		}

		[SetUp]
		public void Setup()
		{
			_dbConnection = new SqliteConnection(SQLITE_CONNECTION_STRING);
			_dbConnection.Open();
			using var ctx = new NorthwindContext(_northwindOptions);
			ctx.Database.EnsureDeleted();
			if (ctx.Database.EnsureCreated())
			{
				NorthwindData.Seed(ctx);
			}
			var options = ctx.GetLinqToDBOptions();
			if (options?.DataContextOptions.Interceptors != null)
			{
				foreach (var interceptor in options.DataContextOptions.Interceptors)
				{
					((TestInterceptor)interceptor).ResetInvocations();
				}
			}

			using var ctx2 = new NorthwindContext(_northwindOptionsWithEfCoreInterceptorsOnly);
			var options2 = ctx2.GetLinqToDBOptions();
			if (options2?.DataContextOptions.Interceptors != null)
			{
				foreach (var interceptor in options2.DataContextOptions.Interceptors)
				{
					((TestInterceptor)interceptor).ResetInvocations();
				}
			}
		}

		[TearDown]
		public void TearDown()
		{
			_dbConnection?.Close();
		}

		[Test]
		public void TestInterceptors()
		{
			using (var ctx = CreateContext())
			{
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToLinqToDB().ToArray();
			}
			Assert.IsTrue(testCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(testConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(testEntityServiceInterceptor.HasInterceptorBeenInvoked);

			//the following check is false because linq2db context is never closed together
			//with the EF core context
			Assert.IsFalse(testDataContextInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestExplicitDataContextInterceptors()
		{
			using (var ctx = CreateContext())
			{
				using var linq2DbContext = ctx.CreateLinqToDbContext();
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToLinqToDB(linq2DbContext).ToArray();
				var items2 = query.Take(2).ToLinqToDB(linq2DbContext).ToArray();
			}
			Assert.IsTrue(testCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(testDataContextInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(testConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(testEntityServiceInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestEfCoreSideOfComboInterceptor()
		{
			using (var ctx = CreateContextWithountLinq2DbExtensions())
			{
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToArray();
			}
			Assert.IsTrue(testEfCoreAndLinq2DbInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestLinq2DbSideOfComboInterceptor()
		{
			using (var ctx = CreateContextWithountLinq2DbExtensions())
			{
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToLinqToDB().ToArray();
			}
			Assert.IsTrue(testEfCoreAndLinq2DbInterceptor.HasInterceptorBeenInvoked);
		}
	}
}
