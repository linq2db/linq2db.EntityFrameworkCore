using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind;
using LinqToDB.Expressions;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	[TestFixture]
	public class ToolsTests : TestsBase
	{
		private readonly DbContextOptions _options;
		private DbContextOptions<NorthwindContext> _inmemoryOptions;

		static ToolsTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public ToolsTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=.;Database=NorthwindEFCore;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;

			optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseInMemoryDatabase("sample");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_inmemoryOptions = optionsBuilder.Options;
		}

		private NorthwindContext CreateContextInMemory()
		{
			var ctx = new NorthwindContext(_inmemoryOptions);
			ctx.Database.EnsureCreated();
			return ctx;
		}

		private void SetIdentityInsert(DbContext ctx, string tableName, bool isOn)
		{
			var str = $"SET IDENTITY_INSERT {tableName} " + (isOn ? "ON" : "OFF");
			try
			{
				ctx.Database.ExecuteSqlCommand(str);
			}
			catch (Exception)
			{
				// swallow
			}
		}

		private NorthwindContext CreateContext()
		{
			var ctx = new NorthwindContext(_options);
			if (ctx.Database.EnsureCreated())
			{


				SetIdentityInsert(ctx, "[dbo].[Employees]", true);
				SetIdentityInsert(ctx, "[dbo].[Categories]", true);
				SetIdentityInsert(ctx, "[dbo].[Orders]", true);
				SetIdentityInsert(ctx, "[dbo].[Products]", true);
				SetIdentityInsert(ctx, "[dbo].[Shippers]", true);
				SetIdentityInsert(ctx, "[dbo].[Suppliers]", true);

				try
				{
					NorthwindData.Seed(ctx);
				}
				finally
				{
					SetIdentityInsert(ctx, "[dbo].[Employees]", false);
					SetIdentityInsert(ctx, "[dbo].[Categories]", false);
					SetIdentityInsert(ctx, "[dbo].[Orders]", false);
					SetIdentityInsert(ctx, "[dbo].[Products]", false);
					SetIdentityInsert(ctx, "[dbo].[Shippers]", false);
					SetIdentityInsert(ctx, "[dbo].[Suppliers]", false);
				}

			}			
			return ctx;
		}

		public class VwProductAndDescription
		{
			public int ProductId { get; set; }
			public string Name { get; set; }
			public string ProductModel { get; set; }
			public string Description { get; set; }
		}

		[Test]
		public void TestToList()
		{
			using (var ctx = CreateContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var items = db.GetTable<Order>()
					.LoadWith(d => d.OrderDetails)
					.ThenLoad(d => d.Product).ToList();
			}
		}

		[Test]
		public void TestShadowProperty()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.Products.Select(p => new
				{
					Quantity = EF.Property<string>(p, "QuantityPerUnit")
				});

				var expected = query.ToArray();
				var result = query.ToLinqToDB().ToArray();
			}
		}

		IQueryable<Product> ProductQuery(NorthwindContext ctx)
		{
			return ctx.Products.Where(p => p.OrderDetails.Count > 0);
		}

		[Test]
		public void TestCallback()
		{
			using (var ctx = CreateContext())
			{
				var query = ProductQuery(ctx)
					.Where(pd => pd.ProductName.StartsWith("a"));

				query.Where(p => p.ProductName == "a").Delete();
			}
		}


		[Test]
		public void TestContextRetrieving()
		{
			using (var ctx = CreateContext())
			{
				var query = ProductQuery(ctx)
					.ToLinqToDB()
					.Where(pd => pd.ProductName.StartsWith("a"));
			}
		}

		[Test]
		public void TestDelete()
		{
			using (var ctx = CreateContext())
			{
				var query = ProductQuery(ctx)
					.Where(pd => pd.ProductName.StartsWith("a"));
			}
		}

		[Test]
		public void TestNestingFunctions()
		{
			using (var ctx = CreateContext())
			{
				var query =
					from pd in ProductQuery(ctx)
					from pd2 in ProductQuery(ctx)
					where pd.ProductId == pd2.ProductId
					orderby pd.ProductId
					select new { pd, pd2 };

				var items1 = query.ToArray();
				var items2 = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public void TestCreateFromOptions()
		{
			using (var db = _options.CreateLinqToDbConnection())
			{
			}
		}

		[Test]
		public void TestFunctions()
		{
			using (var ctx = CreateContext())
			{
				var query = from p in ctx.Orders
					//where EF.Functions.Like(p., "a%") || true
					//orderby p.ProductId
					select new
					{
						p.OrderId,
						// Date = Model.TestFunctions.GetDate(),
						// Len = Model.TestFunctions.Len(p.Name),
						DiffYear1 = EF.Functions.DateDiffYear(p.ShippedDate, p.OrderDate),
						DiffYear2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffYear(p.ShippedDate, p.OrderDate.Value),
						DiffMonth1 = EF.Functions.DateDiffMonth(p.ShippedDate, p.OrderDate),
						DiffMonth2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffMonth(p.ShippedDate, p.OrderDate.Value),
						DiffDay1 = EF.Functions.DateDiffDay(p.ShippedDate, p.OrderDate),
						DiffDay2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffDay(p.ShippedDate, p.OrderDate.Value),
						DiffHour1 = EF.Functions.DateDiffHour(p.ShippedDate, p.OrderDate),
						DiffHour2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffHour(p.ShippedDate, p.OrderDate.Value),
						DiffMinute1 = EF.Functions.DateDiffMinute(p.ShippedDate, p.OrderDate),
						DiffMinute2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffMinute(p.ShippedDate, p.OrderDate.Value),
						DiffSecond1 = EF.Functions.DateDiffSecond(p.ShippedDate, p.OrderDate),
						DiffSecond2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffSecond(p.ShippedDate, p.OrderDate.Value),
						DiffMillisecond1 = EF.Functions.DateDiffMillisecond(p.ShippedDate, p.ShippedDate.Value.AddMilliseconds(100)),
						DiffMillisecond2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffMillisecond(p.ShippedDate, p.ShippedDate.Value.AddMilliseconds(100)),
					};

//				var items1 = query.ToArray();
				var items2 = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public async Task TestTransaction()
		{
			using (var ctx = CreateContext())
			{
				using (var transaction = ctx.Database.BeginTransaction())
				using (var db = ctx.CreateLinqToDbConnection())
				{

					var test1 = await ctx.Products.Where(p => p.ProductName.StartsWith("U")).MaxAsync(p => p.QuantityPerUnit);
					var test2 = await ctx.Products.Where(p => p.ProductName.StartsWith("U")).MaxAsyncLinqToDB(p => p.QuantityPerUnit);

					Assert.AreEqual(test1, test2);

					ctx.Products.Where(p => p.ProductName == "a")
						.ToLinqToDB(db)
						.Delete();

					transaction.Rollback();
				}
			}
		}

		[Test]
		public void TestView()
		{
			using (var ctx = CreateContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var query = ProductQuery(ctx)
					.ToLinqToDB(db)
					.Where(pd => pd.ProductName.StartsWith("a"));

				var items = query.ToArray();
			}
		}


		[Test]
		public void TestTransformation()
		{
			using (var ctx = CreateContext())
			{
				var query =
					from p in ctx.Products
					from c in ctx.Categories.ToLinqToDBTable().InnerJoin(c => c.CategoryId == p.CategoryId)
					select new
					{
						Product = p,
						Ctegory = c
					};

				var items = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public void TestDemo2()
		{
			using (var ctx = CreateContext())
			{
				var query =
					from p in ctx.Products
					from op in ctx.Products.LeftJoin(op => op.ProductId != p.ProductId && op.ProductName == p.ProductName)
					where Sql.ToNullable(op.ProductId) == null
					select p;

				query = query.ToLinqToDB();

				var str = query.ToString();

				var items = query.ToArray();
			}
		}

		[Test]
		public void TestKey()
		{
			using (var ctx = CreateContext())
			{
				var dependencies  = ctx.GetService<RelationalSqlTranslatingExpressionVisitorDependencies>();
				var mappingSource = ctx.GetService<IRelationalTypeMappingSource>();
				var converters    = ctx.GetService<IValueConverterSelector>();
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model, converters, dependencies, mappingSource);
				
				var customerPk = ms.GetAttribute<ColumnAttribute>(typeof(Customer),
					MemberHelper.MemberOf<Customer>(c => c.CustomerId));

				Assert.NotNull(customerPk);
				Assert.AreEqual(true, customerPk.IsPrimaryKey);
				Assert.AreEqual(0, customerPk.PrimaryKeyOrder);

			}
		}

		[Test]
		public void TestAssociations()
		{
			using (var ctx = CreateContext())
			{
				var dependencies = ctx.GetService<RelationalSqlTranslatingExpressionVisitorDependencies>();
				var mappingSource = ctx.GetService<IRelationalTypeMappingSource>();
				var converters    = ctx.GetService<IValueConverterSelector>();
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model, converters, dependencies, null);
				
				var associationOrder = ms.GetAttribute<AssociationAttribute>(typeof(Customer),
					MemberHelper.MemberOf<Customer>(c => c.Orders));

				Assert.NotNull(associationOrder);
				Assert.That(associationOrder.ThisKey, Is.EqualTo("CustomerId"));
				Assert.That(associationOrder.OtherKey, Is.EqualTo("CustomerId"));
			}
		}


		[Test]
		public void TestIdentityColumn()
		{
			using (var ctx = CreateContext())
			{
				var dependencies  = ctx.GetService<RelationalSqlTranslatingExpressionVisitorDependencies>();
				var mappingSource = ctx.GetService<IRelationalTypeMappingSource>();
				var converters    = ctx.GetService<IValueConverterSelector>();
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model, converters, dependencies, mappingSource);
				
				var identity = ms.GetAttribute<ColumnAttribute>(typeof(Customer),
					MemberHelper.MemberOf<Customer>(c => c.CustomerId));

				//TODO:
				//Assert.NotNull(identity);
				//Assert.AreEqual(true, identity.IsIdentity);
			}
		}

		[Test]
		public void TestGlobalQueryFilters([Values(true, false)] bool disableFilter)
		{
			using (var ctx = CreateContext())
			{
				ctx.IsSoftDeleteFilterEnabled = !disableFilter;

				var withoutFilterQuery =
					from p in ctx.Products.IgnoreQueryFilters()
					join d in ctx.OrderDetails on p.ProductId equals d.ProductId
					select new { p, d };

				var efResult      = withoutFilterQuery.ToArray();
				var linq2dbResult = withoutFilterQuery.ToLinqToDB().ToArray();

				Assert.AreEqual(efResult.Length, linq2dbResult.Length);

				var withFilterQuery =
					from p in ctx.Products
					join d in ctx.OrderDetails on p.ProductId equals d.ProductId
					select new { p, d };

				efResult      = withFilterQuery.ToArray();
				linq2dbResult = withFilterQuery.ToLinqToDB().ToArray();

				Assert.AreEqual(efResult.Length, linq2dbResult.Length);
			}
		}


		[Test]
		public async Task TestAsyncMethods()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.Products.AsQueryable().Where(p => p.ProductName.Contains("a"));

				var expected = await query.ToArrayAsync();
				var expectedDictionary = await query.ToDictionaryAsync(p => p.ProductId);
				var expectedAny = await query.AnyAsync();

				var byList = await EntityFrameworkQueryableExtensions.ToListAsync(query.ToLinqToDB());
				var byArray = await EntityFrameworkQueryableExtensions.ToArrayAsync(query.ToLinqToDB());
				var byDictionary = await EntityFrameworkQueryableExtensions.ToDictionaryAsync(query.ToLinqToDB(), p => p.ProductId);
				var any = await EntityFrameworkQueryableExtensions.AnyAsync(query.ToLinqToDB());
			}
		}

		[Test]
		public async Task TestInclude()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.Orders
					.Include(o => o.Employee)
					.ThenInclude(e => e.EmployeeTerritories)
					.ThenInclude(et => et.Territory)
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}

		[Test]
		public async Task TestIncludeString()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.Orders
					.Include("Employee.EmployeeTerritories")
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}
		[Test]
		public async Task TestLoadFilter()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.Products.Select(p => new
					{
						p.ProductName,
						OrderDetails = p.OrderDetails.Select(od => new
						{
							od.Discount,
							od.Order,
							od.Product.Supplier.Products
						})
					});

				ctx.IsSoftDeleteFilterEnabled = true;

				var expected = await query.ToArrayAsync();
				var filtered = await query.ToLinqToDB().ToArrayAsync();

				Assert.That(filtered.Length, Is.EqualTo(expected.Length));
			}
		}


		[Test]
		public async Task TestGetTable()
		{
			using (var ctx = CreateContext())
			{
				var query = ctx.GetTable<Customer>()
					.Where(o => o.City != null);

				var expected = await query.ToArrayAsync();
			}
		}

		[Test]
		public void TestInMemory()
		{
			using (var ctx = CreateContextInMemory())
			{
				Assert.Throws<LinqToDBForEFToolsException>(() =>
				{
					ctx.Products.ToLinqToDB().ToArray();
				});

				Assert.Throws<LinqToDBForEFToolsException>(() =>
				{
					ctx.Products
						.Where(so => so.ProductId == -1)
						.Delete();
				});
			}
		}

		[Test]
		public async Task TestContinuousQueries()
		{
			Common.Configuration.Linq.AllowMultipleQuery = true;

			using (var ctx = CreateContext())
			{
				var query = ctx.Orders
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product)
					.ThenInclude(p => p.OrderDetails);

				var expected = await query.ToArrayAsync();
				var result = await query.ToLinqToDB().ToArrayAsync();
			}

		}

		[Test]
		public async Task TestSetUpdate()
		{
			using (var ctx = CreateContext())
			{
				var customer = await ctx.Customers.FirstOrDefaultAsync();

				var updatable = ctx.Customers.Where(c => c.CustomerId == customer.CustomerId)
					.Set(c => c.CompanyName, customer.CompanyName);

				var affected = updatable
					.UpdateAsync();
			}
		}

		[Test]
		public async Task FromSqlRaw()
		{
			using (var ctx = CreateContext())
			{
				var id = 1;
				var query = ctx.Categories.FromSqlRaw("SELECT * FROM [dbo].[Categories] WHERE CategoryId = {0}", id);


				var efResult = await query.ToArrayAsyncEF();
				var linq2dbResult = await query.ToArrayAsyncLinqToDB();
			}
		}

		[Test]
		public async Task FromSqlRaw2()
		{
			using (var ctx = CreateContext())
			{
				var id = 1;
				var query = from c1 in ctx.Categories
					from c2 in ctx.Categories.FromSqlRaw("SELECT * FROM [dbo].[Categories] WHERE CategoryId = {0}", id)
					select c2;

				var efResult = await query.ToArrayAsyncEF();
				var linq2dbResult = await query.ToArrayAsyncLinqToDB();
			}
		}

		[Test]
		public async Task FromSqlInterpolated()
		{
			using (var ctx = CreateContext())
			{
				var id = 1;
				var query = ctx.Categories.FromSqlInterpolated($"SELECT * FROM [dbo].[Categories] WHERE CategoryId = {id}");

				var efResult = await query.ToArrayAsyncEF();
				var linq2dbResult = await query.ToArrayAsyncLinqToDB();
			}
		}

		[Test]
		public async Task FromSqlInterpolated2()
		{
			using (var ctx = CreateContext())
			{
				var id = 1;
				var query = from c1 in ctx.Categories
					from c2 in ctx.Categories.FromSqlInterpolated($"SELECT * FROM [dbo].[Categories] WHERE CategoryId = {id}")
					select c2;

				var efResult = await query.ToArrayAsyncEF();
				var linq2dbResult = await query.ToArrayAsyncLinqToDB();
			}
		}

	}
}
