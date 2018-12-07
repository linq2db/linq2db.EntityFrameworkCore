using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using LinqToDB.Expressions;
using LinqToDB.Mapping;
using NUnit.Framework;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using LinqToDB;
using LinqToDB.EntityFrameworkCore.Tests;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	[TestFixture]
	public class ToolsTests : TestsBase
	{
		private readonly DbContextOptions _options;
		private DbContextOptions<AdventureWorksContext> _inmemoryOptions;

		static ToolsTests()
		{
			LinqToDBForEFTools.Initialize();
			DataConnection.TurnTraceSwitchOn();
		}

		public ToolsTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=.;Database=AdventureWorks;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;

			optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseInMemoryDatabase("sample");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_inmemoryOptions = optionsBuilder.Options;
		}

		private AdventureWorksContext CreateAdventureWorksContextInMemory()
		{
			var ctx = new AdventureWorksContext(_inmemoryOptions);
			ctx.Database.EnsureCreated();
			return ctx;
		}

		private AdventureWorksContext CreateAdventureWorksContext()
		{
			var ctx = new AdventureWorksContext(_options);
			ctx.Database.EnsureCreated();
			return ctx;
		}

		public class VwProductAndDescription
		{
			public int ProductID { get; set; }
			public string Name { get; set; }
			public string ProductModel { get; set; }
			public string Description { get; set; }
		}

		private IQueryable<VwProductAndDescription> ViewProductAndDescription(AdventureWorksContext ctx)
		{
			var query =
				from p in ctx.Products.AsNoTracking()
				from pmx in p.ProductModel.ProductModelProductDescription
				select new VwProductAndDescription
				{
					ProductID = p.ProductID,
					Name = p.Name,
					ProductModel = p.ProductModel.Name,
					Description = pmx.ProductDescription.Description
				};
			return query;
		}


		[Test]
		public void TestToList()
		{
			using (var ctx = CreateAdventureWorksContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var items = db.GetTable<SalesOrderDetail>().LoadWith(d => d.SalesOrder).ToList();
			}
		}


		[Test]
		public void TestInsertFrom()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				ctx.Products.Where(p => p.Name.StartsWith("Doubled - ")).Delete();


				// all items that have more than 2 products with the same ProductModel
				IQueryable<Product> itemsToInsert = from p in ctx.Products
					group p by new { p.ProductModel }
					into g
					where g.Count() > 2
					join p2 in ctx.Products on g.Key.ProductModel equals p2.ProductModel
					select p2;

				// create duplicate
				var affectedRecords = itemsToInsert.Insert(ctx.Products.ToLinqToDBTable(), s => new Product
				{
					Name = "Doubled - " + s.Name,
					ProductModelID = s.ProductModelID,
					Size = s.Size,
					Color = s.Color,
					DiscontinuedDate = s.DiscontinuedDate,
					ListPrice = s.ListPrice,
					SellStartDate = s.SellStartDate,
					SellEndDate = s.SellEndDate,
					StandardCost = s.StandardCost,
					ProductCategoryID = s.ProductCategoryID,
					ProductNumber = "D-" + s.ProductNumber,
					ThumbnailPhotoFileName = s.ThumbnailPhotoFileName,
					ThumbNailPhoto = s.ThumbNailPhoto,
					Weight = s.Weight,
					ModifiedDate = s.ModifiedDate,
				});

				itemsToInsert.Update(prev => new Product { Name = "U_" + prev.Name });
				var duplicatedRecords = itemsToInsert.Where(p => p.Name.StartsWith("Doubled - "));

				// delete duplicates !!
				duplicatedRecords.Delete();
			}
		}

		[Test]
		public void DemoTest()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var productsWithModelCount =
					from p in ctx.Products
					select new
					{
						Count = Sql.Ext.Count().Over().PartitionBy(p.ProductModelID).ToValue(),
						Product = p
					};

				var neededrecords =
					from p in productsWithModelCount
					where p.Count.Between(2, 4)
					select new
					{
						p.Product.Name,
						p.Product.Color,
						p.Product.Size,
						PhotoFileName = Sql.Property<string>(p.Product, "ThumbnailPhotoFileName")
					};

				var items1 = neededrecords.ToLinqToDB().ToArray();
				var items2 = neededrecords.ToArrayAsyncLinqToDB().Result;
			}
			
		}

		[Test]
		public void TestCallback()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ViewProductAndDescription(ctx)
					.Where(pd => pd.Description.StartsWith("a"));

				query.Where(p => p.Name == "a").Delete();
			}
		}


		[Test]
		public void TestContextRetrieving()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ViewProductAndDescription(ctx)
					.ToLinqToDB()
					.Where(pd => pd.Description.StartsWith("a"));


				var items = query.ToArray();

				query.Where(p => p.Name == "a").Delete();
			}
		}

		[Test]
		public void TestDelete()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ViewProductAndDescription(ctx)
					.Where(pd => pd.Description.StartsWith("a"));

				query.Where(p => p.Name == "a").Delete();
			}
		}

		[Test]
		public void TestNestingFunctions()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query =
					from pd in ViewProductAndDescription(ctx)
					from pd2 in ViewProductAndDescription(ctx)
					where pd.ProductID == pd2.ProductID
					orderby pd.ProductID
					select new { pd, pd2 };

				var zz1 = ViewProductAndDescription(ctx).ToArray();

				var zz2 = ViewProductAndDescription(ctx).ToArray();

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
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = from p in ctx.Products
					where EF.Functions.Like(p.Name, "a%") || true
					orderby p.ProductID
					select new
					{
						p.ProductID,
						Date = Model.TestFunctions.GetDate(),
						Len = Model.TestFunctions.Len(p.Name),
						DiffYear1 = EF.Functions.DateDiffYear(p.SellStartDate, p.SellEndDate),
						DiffYear2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffYear(p.SellStartDate, p.SellEndDate.Value),
						DiffMonth1 = EF.Functions.DateDiffMonth(p.SellStartDate, p.SellEndDate),
						DiffMonth2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffMonth(p.SellStartDate, p.SellEndDate.Value),
						DiffDay1 = EF.Functions.DateDiffDay(p.SellStartDate, p.SellEndDate),
						DiffDay2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffDay(p.SellStartDate, p.SellEndDate.Value),
						DiffHour1 = EF.Functions.DateDiffHour(p.SellStartDate, p.SellEndDate),
						DiffHour2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffHour(p.SellStartDate, p.SellEndDate.Value),
						DiffMinute1 = EF.Functions.DateDiffMinute(p.SellStartDate, p.SellEndDate),
						DiffMinute2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffMinute(p.SellStartDate, p.SellEndDate.Value),
						DiffSecond1 = EF.Functions.DateDiffSecond(p.SellStartDate, p.SellEndDate),
						DiffSecond2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffSecond(p.SellStartDate, p.SellEndDate.Value),
						DiffMillisecond1 = EF.Functions.DateDiffMillisecond(p.SellStartDate, p.SellStartDate.AddMilliseconds(100)),
						DiffMillisecond2 = p.SellEndDate == null ? (int?)null : EF.Functions.DateDiffMillisecond(p.SellStartDate, p.SellStartDate.AddMilliseconds(100)),
					};

				var items1 = query.ToArray();
				var items2 = query.ToLinqToDB().ToArray();
			}
		}

		[Test]
		public async Task TestTransaction()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				using (var transaction = ctx.Database.BeginTransaction())
				using (var db = ctx.CreateLinqToDbConnection())
				{
					var items1 = ViewProductAndDescription(ctx)
						.ToLinqToDB(db)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					var items2 = ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.Where(p => p.Name == "a")
						.ToLinqToDB(db)
						.Delete();


					var test1 = await ctx.Products.Where(p => p.Name.StartsWith("U")).MaxAsync(p => p.StandardCost);
					var test2 = await ctx.Products.Where(p => p.Name.StartsWith("U")).MaxAsyncLinqToDB(p => p.StandardCost);

					Assert.AreEqual(test1, test2);

					ctx.Products.Where(p => p.Name == "a")
						.ToLinqToDB(db)
						.Delete();

					transaction.Rollback();
				}
			}
		}

		[Test]
		public void TestView()
		{
			using (var ctx = CreateAdventureWorksContext())
			using (var db = ctx.CreateLinqToDbConnection())
			{
				var query = ViewProductAndDescription(ctx)
					.ToLinqToDB(db)
					.Where(pd => pd.Description.StartsWith("a"));

				var items = query.ToArray();

				query.Where(p => p.Name == "a").Delete();
			}
		}


		[Test]
		public void TestTransformation()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query =
					from p in ctx.Products
					from c in ctx.ProductCategories.ToLinqToDBTable().InnerJoin(c => c.ProductCategoryID == p.ProductCategoryID)
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
			using (var ctx = CreateAdventureWorksContext())
			{
				var query =
					from p in ctx.Products
					from op in ctx.Products.LeftJoin(op => op.ProductID != p.ProductID && op.Name == p.Name)
					where Sql.ToNullable(op.ProductID) == null
					select p;

				query = query.ToLinqToDB();

				var str = query.ToString();

				var items = query.ToArray();
			}
		}

		[Test]
		public void TestCompositeKey()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model);
				
				var customerPk = ms.GetAttribute<ColumnAttribute>(typeof(CustomerAddress),
					MemberHelper.MemberOf<CustomerAddress>(c => c.CustomerID));

				Assert.NotNull(customerPk);
				Assert.AreEqual(true, customerPk.IsPrimaryKey);
				Assert.AreEqual(0, customerPk.PrimaryKeyOrder);
				
				var addressPk = ms.GetAttribute<ColumnAttribute>(typeof(CustomerAddress),
					MemberHelper.MemberOf<CustomerAddress>(c => c.AddressID));

				Assert.NotNull(addressPk);
				Assert.AreEqual(true, addressPk.IsPrimaryKey);
				Assert.AreEqual(1, addressPk.PrimaryKeyOrder);
			}
		}

		[Test]
		public void TestAssociations()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model);
				
				var associationCustomer = ms.GetAttribute<AssociationAttribute>(typeof(CustomerAddress),
					MemberHelper.MemberOf<CustomerAddress>(c => c.Customer));

				Assert.NotNull(associationCustomer);
				Assert.AreEqual("CustomerID", associationCustomer.ThisKey);
				Assert.AreEqual("CustomerID", associationCustomer.OtherKey);
				
				var associationAddress = ms.GetAttribute<AssociationAttribute>(typeof(CustomerAddress),
					MemberHelper.MemberOf<CustomerAddress>(c => c.Address));

				Assert.NotNull(associationAddress);
				Assert.AreEqual("AddressID", associationAddress.ThisKey);
				Assert.AreEqual("AddressID", associationAddress.OtherKey);
			}
		}


		[Test]
		public void TestIdentityColumn()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var ms = LinqToDBForEFTools.GetMappingSchema(ctx.Model);
				
				var identity = ms.GetAttribute<ColumnAttribute>(typeof(SalesOrderDetail),
					MemberHelper.MemberOf<SalesOrderDetail>(c => c.SalesOrderDetailID));

				Assert.NotNull(identity);
				Assert.AreEqual(true, identity.IsIdentity);
			}
		}

		[Test]
		public void TestGlobalQueryFilters()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var withoutFilter = ctx.Products.IgnoreQueryFilters().ToLinqToDB().ToArray();
				var products = ctx.Products.ToLinqToDB().ToArray();

				Assert.AreNotEqual(withoutFilter.Length, products.Length);
			}
		}


		[Test]
		public async Task TestAsyncMethods()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.Products.AsQueryable().Where(p => p.Name.Contains("a"));

				var expected = await query.ToArrayAsync();
				var expectedDictionary = await query.ToDictionaryAsync(p => p.ProductID);
				var expectedAny = await query.AnyAsync();

				var byList = await EntityFrameworkQueryableExtensions.ToListAsync(query.ToLinqToDB());
				var byArray = await EntityFrameworkQueryableExtensions.ToArrayAsync(query.ToLinqToDB());
				var byDictionary = await EntityFrameworkQueryableExtensions.ToDictionaryAsync(query.ToLinqToDB(), p => p.ProductID);
				var any = await EntityFrameworkQueryableExtensions.AnyAsync(query.ToLinqToDB());
			}
		}

		[Test]
		public async Task TestInclude()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.SalesOrderDetails
					.Include(d => d.SalesOrder)
					.Include(d => d.Product)
					.ThenInclude(p => p.ProductCategory);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}

		[Test]
		public async Task TestIncludeString()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.SalesOrderDetails
					.Include("SalesOrder")
					.Include(d => d.Product)
					.ThenInclude(p => p.ProductCategory);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}
		}


		[Test]
		public async Task TestIncludeMany()
		{
			Common.Configuration.Linq.AllowMultipleQuery = true;

			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.SalesOrders
					.Include(o => o.Details)
					.ThenInclude(d => d.SalesOrder);

				var expected = await query.ToArrayAsync();

				var result = await query.ToLinqToDB().ToArrayAsync();
			}

		}

		[Test]
		public async Task TestGetTable()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.GetTable<SalesOrder>()
					.Where(o => o.IsOnlineOrder);

				var expected = await query.ToArrayAsync();
			}
		}

		[Test]
		public void TestInMemory()
		{
			using (var ctx = CreateAdventureWorksContextInMemory())
			{
				Assert.Throws<LinqToDBForEFToolsException>(() =>
				{
					ctx.SalesOrders.ToLinqToDB().ToArray();
				});

				Assert.Throws<LinqToDBForEFToolsException>(() =>
				{
					var query = ctx.SalesOrders
						.Where(so => so.CustomerID == -1)
						.Delete();
				});
			}
		}

		[Test]
		public async Task TestContinuousQueries()
		{
			Common.Configuration.Linq.AllowMultipleQuery = true;

			using (var ctx = CreateAdventureWorksContext())
			{
				var query = ctx.SalesOrders
					.Include(o => o.Details)
					.ThenInclude(d => d.SalesOrder);

				var expected = await query.ToLinqToDB().ToArrayAsync();
				var result = await query.ToLinqToDB().ToArrayAsync();
			}

		}

		[Test]
		public async Task TestSetUpdate()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				var customer = await ctx.Customers.FirstOrDefaultAsync();

				var updatable = ctx.Customers.Where(c => c.CustomerID == customer.CustomerID)
					.Set(c => c.CompanyName, customer.CompanyName);

				var affected = updatable
					.UpdateAsync();
			}
		}


	}
}
