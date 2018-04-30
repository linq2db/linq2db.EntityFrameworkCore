using System;
using System.Linq;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	[TestFixture]
	public class ToolsTests
	{
		private readonly DbContextOptions _options;

		static ToolsTests()
		{
			LinqToDBForEFTools.Initialize();

			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (s, s1) => Console.WriteLine(s, s1);
		}

		public ToolsTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=OCEANIA;Database=AdventureWorks;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) }));

			_options = optionsBuilder.Options;
		}

		private AdventureWorksContext CreateAdventureWorksContext()
		{
			var ctx = new AdventureWorksContext(_options);
			ctx.Database.EnsureCreated();
			return ctx;
		}

		public class vwProductAndDescription
		{
			public int ProductID { get; set; }
			public string Name { get; set; }
			public string ProductModel { get; set; }
			public string Description { get; set; }
		}

		private IQueryable<vwProductAndDescription> ViewProductAndDescription(AdventureWorksContext ctx)
		{
			var query =
				from p in ctx.Products.AsNoTracking()
				from pmx in p.ProductModel.ProductModelProductDescription
				select new vwProductAndDescription
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
				var items = ctx.CustomerAddresses.ToListAsyncLinqToDB().Result;

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

				var duplicatedRecords = itemsToInsert.Where(p => p.Name.StartsWith("Doubled - "));

				// delete duplicates !!
				duplicatedRecords.Delete();
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
					.ToLinqToDb()
					.Where(pd => pd.Description.StartsWith("a"));


				var items = query.ToArray();

				query.Where(p => p.Name == "a").Delete();
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
					select new
					{
						p.ProductID,
						Date = Model.TestFunctions.GetDate(),
						Len = Model.TestFunctions.Len(p.Name)
					};

				var items1 = query.ToArray();
				var items2 = query.ToLinqToDb().ToArray();
			}
		}

		[Test]
		public void TestTransaction()
		{
			using (var ctx = CreateAdventureWorksContext())
			{
				using (var transaction = ctx.Database.BeginTransaction())
				using (var db = ctx.CreateLinqToDbConnection())
				{
					var items1 = ViewProductAndDescription(ctx)
						.ToLinqToDb(db)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					var items2 = ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.Where(p => p.Name == "a")
						.ToLinqToDb(db)
						.Delete();



					var test1 = ctx.Products.Where(p => p.Name.StartsWith("a")).MaxAsync(p => p.StandardCost).Result;
					var test2 = ctx.Products.Where(p => p.Name.StartsWith("a")).ToLinqToDb().MaxAsync(p => p.StandardCost).Result;

					Assert.AreEqual(test1, test2);

					ctx.Products.Where(p => p.Name == "a")
						.ToLinqToDb(db)
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
					.ToLinqToDb(db)
					.Where(pd => pd.Description.StartsWith("a"));

				var items = query.ToArray();

				query.Where(p => p.Name == "a").Delete();
			}
		}
	}
}
