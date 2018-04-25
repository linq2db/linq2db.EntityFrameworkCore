using System;
using System.Data.Common;
using System.Linq;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using NUnit.Framework;
using LinqToDB.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	[TestFixture]
    public class ToolsTests
    {
	    private static DbContextOptions _options;

	    static ToolsTests()
	    {
			DataConnection.TurnTraceSwitchOn();
		    DataConnection.WriteTraceLine = (s, s1) => Console.WriteLine(s, s1);

		    var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
		    //new SqlServerDbContextOptionsBuilder(optionsBuilder);

		    optionsBuilder.UseSqlServer("Server=OCEANIA;Database=AdventureWorks;Integrated Security=SSPI");

		    _options = optionsBuilder.Options;
	    }



	    private AdventureWorksContext CreateAdventureWorksContext()
	    {
		    var ctx = new AdventureWorksContext(_options);
		    ctx.Database.EnsureCreated();
		    return ctx;
	    }

		[Test]  
	    public void Test()
	    {
		    using (var ctx = CreateAdventureWorksContext())
		    using (var db = Linq2DbTools.CreateLinqToDbConnection(ctx))
		    {
			    var items = db.GetTable<SalesOrderDetail>().LoadWith(d => d.SalesOrder).ToList();
		    }
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


		[Test]
	    public void TestCreateFromOptions()
	    {
		    using (var db = _options.CreateLinqToDbConnection())
		    {
		    }
	    }

    }
}

