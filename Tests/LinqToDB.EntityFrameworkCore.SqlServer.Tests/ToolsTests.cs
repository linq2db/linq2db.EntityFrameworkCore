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
				ctx.Database.ExecuteSqlRaw(str);
			}
			catch (Exception)
			{
				// swallow
			}
		}

		private NorthwindContext CreateContext(bool enableFilter)
		{
			var ctx = new NorthwindContext(_options);
			ctx.IsSoftDeleteFilterEnabled = enableFilter;
			//ctx.Database.EnsureDeleted();
			if (ctx.Database.EnsureCreated())
			{
				NorthwindData.Seed(ctx);
			}			
			return ctx;
		}

		public class VwProductAndDescription
		{
			public int ProductId { get; set; }
			public string Name { get; set; } = null!;
			public string ProductModel { get; set; } = null!;
			public string Description { get; set; } = null!;
		}

		[Test]
		public void TestToList([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var items = db.GetTable<Order>()
					.LoadWith(d => d.OrderDetails)
					.ThenLoad(d => d.Product).ToList();
			}
		}

		[Test]
		public void TestShadowProperty([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
		public void TestCallback([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ProductQuery(ctx)
					.Where(pd => pd.ProductName.StartsWith("a"));

				query.Where(p => p.ProductName == "a").Delete();
			}
		}


		[Test]
		public void TestContextRetrieving([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ProductQuery(ctx)
					.ToLinqToDB()
					.Where(pd => pd.ProductName.StartsWith("a"));
			}
		}

		[Test]
		public void TestDelete([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ProductQuery(ctx)
					.Where(pd => pd.ProductName.StartsWith("a"));
			}
		}

		[Test]
		public void TestNestingFunctions([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
			using (var ctx = CreateContext(false))
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
						DiffMillisecond1 = EF.Functions.DateDiffMillisecond(p.ShippedDate, p.ShippedDate!.Value.AddMilliseconds(100)),
						DiffMillisecond2 = p.OrderDate == null ? (int?)null : EF.Functions.DateDiffMillisecond(p.ShippedDate, p.ShippedDate.Value.AddMilliseconds(100)),
					};

//				var items1 = query.ToArray();
				var items2 = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public async Task TestTransaction([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
		public void TestView([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var query = ProductQuery(ctx)
					.ToLinqToDB(db)
					.Where(pd => pd.ProductName.StartsWith("a"));

				var items = query.ToArray();
			}
		}


		[Test]
		public void TestTransformation([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query =
					from p in ctx.Products
					from c in ctx.Categories.InnerJoin(c => c.CategoryId == p.CategoryId)
					select new
					{
						Product = p,
						Ctegory = c
					};

				var items = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public void TestTransformationTable([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
		public void TestDemo2([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
			using (var ctx = CreateContext(false))
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model, ctx);
				
				var customerPk = ms.GetAttribute<ColumnAttribute>(typeof(Customer),
					MemberHelper.MemberOf<Customer>(c => c.CustomerId));

				Assert.NotNull(customerPk);
				Assert.AreEqual(true, customerPk!.IsPrimaryKey);
				Assert.AreEqual(0, customerPk.PrimaryKeyOrder);

			}
		}

		[Test]
		public void TestAssociations()
		{
			using (var ctx = CreateContext(false))
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model, ctx);
				
				var associationOrder = ms.GetAttribute<AssociationAttribute>(typeof(Customer),
					MemberHelper.MemberOf<Customer>(c => c.Orders));

				Assert.NotNull(associationOrder);
				Assert.That(associationOrder!.ThisKey, Is.EqualTo("CustomerId"));
				Assert.That(associationOrder.OtherKey, Is.EqualTo("CustomerId"));
			}
		}


		[Repeat(2)]
		[Test]
		public void TestGlobalQueryFilters([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
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

				var efResult2  = withFilterQuery.ToArray();
				var linq2dbResult2 = withFilterQuery.ToLinqToDB().ToArray();

				Assert.AreEqual(efResult2.Length, linq2dbResult2.Length);
			}
		}


		[Test]
		public async Task TestAsyncMethods([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Products.AsQueryable().Where(p => p.ProductName.Contains("a"));

				var expectedArray = await query.ToArrayAsync();
				var expectedDictionary = await query.ToDictionaryAsync(p => p.ProductId);
				var expectedAny = await query.AnyAsync();

				var byList = await EntityFrameworkQueryableExtensions.ToListAsync(query.ToLinqToDB());
				var byArray = await EntityFrameworkQueryableExtensions.ToArrayAsync(query.ToLinqToDB());
				var byDictionary = await EntityFrameworkQueryableExtensions.ToDictionaryAsync(query.ToLinqToDB(), p => p.ProductId);
				var any = await EntityFrameworkQueryableExtensions.AnyAsync(query.ToLinqToDB());
			}
		}

		[Test]
		public async Task TestInclude([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Orders
					.Include(o => o.Employee!)
					.ThenInclude(e => e.EmployeeTerritories)
					.ThenInclude(et => et.Territory)
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}

		[Test]
		public async Task TestEager([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Orders.Select(o => new
				{
					Employee = o.Employee,
					EmployeeTerritories = o.Employee!.EmployeeTerritories.Select(et => new
					{
						EmployeeTerritory = et,
						Territory = et.Territory
					}),

					OrderDetails = o.OrderDetails.Select(od => new
					{
						OrderDetail = od,
						od.Product
					})
				});

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}

		[Test]
		public async Task TestIncludeString([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
		public async Task TestLoadFilter([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Products.Select(p => new
					{
						p.ProductName,
						OrderDetails = p.OrderDetails.Select(od => new
						{
							od.Discount,
							od.Order,
							od.Product.Supplier!.Products
						})
					});

				ctx.IsSoftDeleteFilterEnabled = true;

				var expected = await query.ToArrayAsync();
				var filtered = await query.ToLinqToDB().ToArrayAsync();

				Assert.That(filtered.Length, Is.EqualTo(expected.Length));
			}
		}

		[Test]
		public async Task TestGetTable([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
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
		public async Task TestContinuousQueries([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Orders
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product)
					.ThenInclude(p => p.OrderDetails);

				var expected = await query.ToArrayAsync();
				var result   = await query.ToLinqToDB().ToArrayAsync();
			}
		}

		[Test]
		public async Task TestChangeTracker([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Orders
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product)
					.ThenInclude(p => p.OrderDetails);
				
				// var efResult = await query.ToArrayAsync();
				var result = await query.ToLinqToDB().ToArrayAsync();

				var orderDetail = result[0].OrderDetails.First();
				orderDetail.UnitPrice = orderDetail.UnitPrice * 1.1m;

				ctx.ChangeTracker.DetectChanges();
				var changedEntry = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Single();
				ctx.SaveChanges();
			}
		}

		[Test]
		public async Task TestChangeTrackerDisabled1([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var query = ctx.Orders
					.Include(o => o.OrderDetails)
					.ThenInclude(d => d.Product)
					.ThenInclude(p => p.OrderDetails)
					.AsNoTracking();

				// var efResult = await query.ToArrayAsync();
				var result = await query.ToLinqToDB().ToArrayAsync();

				var orderDetail = result[0].OrderDetails.First();
				orderDetail.UnitPrice = orderDetail.UnitPrice * 1.1m;

				ctx.ChangeTracker.DetectChanges();
				var changedEntry = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).SingleOrDefault();
				Assert.AreEqual(changedEntry, null);
				ctx.SaveChanges();
			}
		}

		[Test]
		public async Task TestChangeTrackerDisabled2([Values(true, false)] bool enableFilter)
		{
			LinqToDBForEFTools.EnableChangeTracker = false;
			try
			{
				using (var ctx = CreateContext(enableFilter))
				{
					var query = ctx.Orders
						.Include(o => o.OrderDetails)
						.ThenInclude(d => d.Product)
						.ThenInclude(p => p.OrderDetails);

					// var efResult = await query.ToArrayAsync();
					var result = await query.ToLinqToDB().ToArrayAsync();

					var orderDetail = result[0].OrderDetails.First();
					orderDetail.UnitPrice = orderDetail.UnitPrice * 1.1m;

					ctx.ChangeTracker.DetectChanges();
					var changedEntry = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).SingleOrDefault();
					Assert.AreEqual(changedEntry, null);
					ctx.SaveChanges();
				}
			}
			finally
			{
				LinqToDBForEFTools.EnableChangeTracker = true;
			}
		}

		[Test]
		public void NavigationProperties()
		{
			using (var ctx = CreateContext(false))
			{
				var query =
					from o in ctx.Orders
					from od in o.OrderDetails
					select new
					{
						ProductOrderDetails = od.Product.OrderDetails.Select(d => new {d.OrderId, d.ProductId, d.Quantity }).ToArray(),
						OrderDetail = new { od.OrderId, od.ProductId, od.Quantity },
						Product = new { od.Product.ProductId, od.Product.ProductName }
					};

				var efResult   = query.ToArray();
				var l2dbResult = query.ToLinqToDB().ToArray();
				
				AreEqualWithComparer(efResult, l2dbResult);
			}
		}

		[Test]
		public async Task TestSetUpdate([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var customer = await ctx.Customers.FirstAsync();

				var updatable = ctx.Customers.Where(c => c.CustomerId == customer.CustomerId)
					.Set(c => c.CompanyName, customer.CompanyName);

				var affected = await updatable
					.UpdateAsync();
			}
		}

		[Test]
		public async Task FromSqlRaw()
		{
			using (var ctx = CreateContext(false))
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
			using (var ctx = CreateContext(false))
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
			using (var ctx = CreateContext(false))
			{
				var id = 1;
				var query = ctx.Categories.FromSqlInterpolated($"SELECT * FROM [dbo].[Categories] WHERE CategoryId = {id}");

				var efResult = await query.AsNoTracking().ToArrayAsyncEF();
				var linq2dbResult = await query.ToArrayAsyncLinqToDB();
			}
		}

		[Test]
		public async Task FromSqlInterpolated2()
		{
			using (var ctx = CreateContext(false))
			{
				var id = 1;
				var query = from c1 in ctx.Categories
					from c2 in ctx.Categories.FromSqlInterpolated($"SELECT * FROM [dbo].[Categories] WHERE CategoryId = {id}")
					select c2;

				var efResult = await query.AsNoTracking().ToArrayAsyncEF();
				var linq2dbResult = await query.AsNoTracking().ToArrayAsyncLinqToDB();
			}
		}

		[Test]
		public async Task TestDeleteFrom()
		{
			using (var ctx = CreateContext(false))
			{
				var query = ctx.Customers.Where(x => x.IsDeleted).Take(20);

				var affected = await query
					.Where(x => query
						.Select(y => y.CustomerId)
						.Contains(x.CustomerId) && false
					)
					.ToLinqToDB()
					.DeleteAsync();
			}
		}

		[Test]
		public void TestNullability([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				int? test = 1;
				var query = ctx.Employees.Where(e => e.EmployeeId == test);

				var expected = query.ToArray();
				var actual = query.ToLinqToDB().ToArray();

				AreEqualWithComparer(expected, actual);
			}
		}

		[Test]
		public void TestUpdate([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				int? test = 1;
				ctx.Employees.IgnoreQueryFilters().Where(e => e.EmployeeId == test).Update(x => new Employee
				{
					Address = x.Address

				});
			}
		}

		[Test]
		public async Task TestUpdateAsync([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				int? test = 1;
				await ctx.Employees.IgnoreQueryFilters().Where(e => e.EmployeeId == test).UpdateAsync(x => new Employee
				{
					Address = x.Address

				});
			}
		}

		[Test]
		public void TestCreateTempTable([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				using var db = ctx.CreateLinqToDbContext();
				using var temp = db.CreateTempTable(ctx.Employees, "#TestEmployees");

				Assert.AreEqual(ctx.Employees.Count(), temp.Count());
			}
		}


		[Test]
		public void TestForeignKey([Values(true, false)] bool enableFilter)
		{
			using (var ctx = CreateContext(enableFilter))
			{
				var resultEF = ctx.Employees.Include(e => e.ReportsToNavigation).ToArray();
				var result = ctx.Employees.Include(e => e.ReportsToNavigation).ToLinqToDB().ToArray();

				AreEqual(resultEF, result);
			}
		}


		[Test]
		public void TestCommandTimeout()
		{
			int timeoutErrorCode = -2;     // Timeout Expired
			int commandTimeout = 1;
			int commandExecutionTime = 5;
			var createProcessLongFunctionSql =   // function that takes @secondsNumber seconds
				@"CREATE OR ALTER FUNCTION dbo.[ProcessLong]
					(
						@secondsNumber int
					)
					RETURNS int
					AS
					BEGIN
						declare @startTime datetime = getutcdate()
						while datediff(second, @startTime, getutcdate()) < @secondsNumber
						begin
							set @startTime = @startTime
						end
						return 1
					END";
			var dropProcessLongFunctionSql = @"DROP FUNCTION IF EXISTS [dbo].[ProcessLong]";

			using (var ctx = CreateContext(false))
			{
				try
				{
					ctx.Database.ExecuteSqlRaw(createProcessLongFunctionSql);
					ctx.Database.SetCommandTimeout(commandTimeout);

					var query = from p in ctx.Products
								select NorthwindContext.ProcessLong(commandExecutionTime);

					var exception = Assert.Throws<Microsoft.Data.SqlClient.SqlException>(() =>
					{
						var result = query.ToLinqToDB().First();
					});
					Assert.AreEqual(exception.Number, timeoutErrorCode);
				}
				finally
				{
					ctx.Database.ExecuteSqlRaw(dropProcessLongFunctionSql);
				}
			}
		}
	}
}
