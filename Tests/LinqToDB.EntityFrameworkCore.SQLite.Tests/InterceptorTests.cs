using System.Data.Common;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Interceptors;
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
		private readonly DbContextOptions _northwindOptionsWithoutLinq2DbExtensions;
		private DbConnection? _dbConnection;
		static TestCommandInterceptor defaultCommandInterceptor;
		static TestDataContextInterceptor defaultDataContextInterceptor;
		static TestConnectionInterceptor defaultConnectionInterceptor;
		static TestEntityServiceInterceptor defaultEntityServiceInterceptor;
		static TestEfCoreAndLinq2DbComboInterceptor defaultTestEfCoreAndLinq2DbInterceptor;

		static InterceptorTests()
		{
			defaultCommandInterceptor = new TestCommandInterceptor();
			defaultDataContextInterceptor = new TestDataContextInterceptor();
			defaultConnectionInterceptor = new TestConnectionInterceptor();
			defaultEntityServiceInterceptor = new TestEntityServiceInterceptor();
			defaultTestEfCoreAndLinq2DbInterceptor = new TestEfCoreAndLinq2DbComboInterceptor();
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		static DbContextOptions CreateNorthwindOptions()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.UseLinqToDb(builder => 
			{
				builder.AddInterceptor(defaultCommandInterceptor);
				builder.AddInterceptor(defaultDataContextInterceptor);
				builder.AddInterceptor(defaultConnectionInterceptor);
				builder.AddInterceptor(defaultEntityServiceInterceptor);
			});
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			return optionsBuilder.Options;
		}

		static DbContextOptions CreateNorthwindOptionsWithoutLinq2DbExtensions()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			optionsBuilder.UseSqlite(SQLITE_CONNECTION_STRING);
			optionsBuilder.AddInterceptors(defaultTestEfCoreAndLinq2DbInterceptor);
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			return optionsBuilder.Options;
		}

		public InterceptorTests()
		{
			_northwindOptions = CreateNorthwindOptions();
			_northwindOptionsWithoutLinq2DbExtensions = CreateNorthwindOptionsWithoutLinq2DbExtensions();
		}

		private NorthwindContext CreateContext()
		{
			var ctx = new NorthwindContext(_northwindOptions);
			return ctx;
		}

		private NorthwindContext CreateContextWithountLinq2DbExtensions()
		{
			var ctx = new NorthwindContext(_northwindOptionsWithoutLinq2DbExtensions);
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

			foreach (var interceptor in ctx.GetLinq2DbInterceptors())
			{
				((TestInterceptor)interceptor).ResetInvocations();
			}

			using var ctx2 = new NorthwindContext(_northwindOptionsWithoutLinq2DbExtensions);
			foreach (var interceptor in ctx2.GetLinq2DbInterceptors())
			{
				((TestInterceptor)interceptor).ResetInvocations();
			}
		}

		[TearDown]
		public void TearDown()
		{
			_dbConnection?.Close();
		}

		[Test]
		public void TestDefaultInterceptors()
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
			Assert.IsTrue(defaultCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(defaultConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(defaultEntityServiceInterceptor.HasInterceptorBeenInvoked);

			//the following check is false because linq2db context is never closed together
			//with the EF core context
			Assert.IsFalse(defaultDataContextInterceptor.HasInterceptorBeenInvoked);
		}

		[Test]
		public void TestExplicitDataContextDefaultInterceptors()
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
			Assert.IsTrue(defaultCommandInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(defaultDataContextInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(defaultConnectionInterceptor.HasInterceptorBeenInvoked);
			Assert.IsTrue(defaultEntityServiceInterceptor.HasInterceptorBeenInvoked);
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
			Assert.IsTrue(defaultTestEfCoreAndLinq2DbInterceptor.HasInterceptorBeenInvoked);
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
			Assert.IsTrue(defaultTestEfCoreAndLinq2DbInterceptor.HasInterceptorBeenInvoked);
		}
	}
}
