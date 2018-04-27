using System;
using System.Data.Common;
using System.Linq;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
			LinqToDbForEfTools.Initialize();

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
		public void Test()
	    {
		    using (var ctx = CreateAdventureWorksContext())
			    using (var db = ctx.CreateLinqToDbConnection())
			    {
				var items = db.GetTable<SalesOrderDetail>().LoadWith(d => d.SalesOrder).ToList();
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
				    .ToLinqToDbQuery()
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
				var items2 = query.ToLinqToDbQuery().ToArray();
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
						.ToLinqToDbQuery(db)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					var items2 = ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.ToArray();

					ViewProductAndDescription(ctx)
						.Where(pd => pd.Description.StartsWith("a"))
						.Where(p => p.Name == "a")
						.ToLinqToDbQuery(db)
						.Delete();

					ctx.Products.Where(p => p.Name == "a")
						.ToLinqToDbQuery(db)
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
					.ToLinqToDbQuery(db)
					.Where(pd => pd.Description.StartsWith("a"));


				var items = query.ToArray();

				query.Where(p => p.Name == "a").Delete();
			}
		}
    }
}
