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
		static TestCommandInterceptor _testCommandInterceptor;
		static TestDataContextInterceptor _testDataContextInterceptor;
		static TestConnectionInterceptor _testConnectionInterceptor;
		static TestEntityServiceInterceptor _testEntityServiceInterceptor;
		static TestEfCoreAndLinqToDBComboInterceptor _testEfCoreAndLinqToDBInterceptor;

		static InterceptorTests()
		{
			_testCommandInterceptor = new TestCommandInterceptor();
			_testDataContextInterceptor = new TestDataContextInterceptor();
			_testConnectionInterceptor = new TestConnectionInterceptor();
			_testEntityServiceInterceptor = new TestEntityServiceInterceptor();
			_testEfCoreAndLinqToDBInterceptor = new TestEfCoreAndLinqToDBComboInterceptor();
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		static DbContextOptions CreateNorthwindOptions()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.UseLinqToDB(builder =>
			{
				builder
					.AddInterceptor(_testCommandInterceptor)
					.AddInterceptor(_testDataContextInterceptor)
					.AddInterceptor(_testConnectionInterceptor)
					.AddInterceptor(_testEntityServiceInterceptor)
					.AddInterceptor(_testEfCoreAndLinqToDBInterceptor)
					.AddInterceptor(_testCommandInterceptor); //for checking the aggregated interceptors
			});
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			return optionsBuilder.Options;
		}

		static DbContextOptions CreateNorthwindOptionsWithEfCoreInterceptorsOnly()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.AddInterceptors(_testEfCoreAndLinqToDBInterceptor);
			optionsBuilder.UseLinqToDB(builder => builder.UseEfCoreRegisteredInterceptorsIfPossible());
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

		private NorthwindContext CreateContextWithountLinqToDBExtensions()
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
			Assert.IsTrue(_testCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(_testConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(_testEntityServiceInterceptor.HasInterceptorBeenInvoked);

			//the following check is false because linq2db context is never closed together
			//with the EF core context
			Assert.IsFalse(_testDataContextInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestExplicitDataContextInterceptors()
		{
			using (var ctx = CreateContext())
			{
				using var linqToDBContext = ctx.CreateLinqToDBContext();
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToLinqToDB(linqToDBContext).ToArray();
				var items2 = query.Take(2).ToLinqToDB(linqToDBContext).ToArray();
			}
			Assert.IsTrue(_testCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(_testDataContextInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(_testConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(_testEntityServiceInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestEfCoreSideOfComboInterceptor()
		{
			using (var ctx = CreateContextWithountLinqToDBExtensions())
			{
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToArray();
			}
			Assert.IsTrue(_testEfCoreAndLinqToDBInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestLinqToDBSideOfComboInterceptor()
		{
			using (var ctx = CreateContextWithountLinqToDBExtensions())
			{
				var query =
					from pd in ctx.Products
					where pd.ProductId > 0
					orderby pd.ProductId
					select pd;
				var items = query.Take(2).ToLinqToDB().ToArray();
			}
			Assert.IsTrue(_testEfCoreAndLinqToDBInterceptor.HasInterceptorBeenInvoked);
		}
	}
}
