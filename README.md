# linq2db.EntityFrameworkCore

`linq2db.EntityFrameworkCore` is an integration of `LINQ To DB` with existing EntityFrameworkCore projects. It was inspired by [this issue](https://github.com/aspnet/EntityFrameworkCore/issues/11657) in EF.Core repository.

## Build status

| |Appveyor|
-----|-------
|master|[![Master](https://ci.appveyor.com/api/projects/status/vmp9pj4gqrch4x3x/branch/master?svg=true)](https://ci.appveyor.com/project/igor-tkachev/linq2db-entityframeworkcore/branch/master)
|latest|[![Latest](https://ci.appveyor.com/api/projects/status/vmp9pj4gqrch4x3x?svg=true)](https://ci.appveyor.com/project/igor-tkachev/linq2db-entityframeworkcore)

## Feeds

* NuGet [![NuGet](https://img.shields.io/nuget/vpre/linq2db.EntityFrameworkCore.svg)](https://www.nuget.org/packages?q=linq2db)
* MyGet [![MyGet](https://img.shields.io/myget/linq2db/vpre/linq2db.EntityFrameworkCore.svg)](https://www.myget.org/gallery/linq2db)
  * V2 `https://www.myget.org/F/linq2db/api/v2`
  * V3 `https://www.myget.org/F/linq2db/api/v3/index.json`

# How to use

As it is an early preview, it available only from MyGet. Later we'll publish stable version on NuGet.

In your code you need to initialize integration using following call:

```cs
LinqToDBForEFTools.Initialize();
```

After that you can just call DbContext and IQueryable extension methods, provided by `LINQ To DB`.

There are many extensions for CRUD Operations missing in vanilla EF ([watch our video](https://www.youtube.com/watch?v=m--oX73EGeQ)):

```cs
// fast insert big recordsets
ctx.BulkCopy(new BulkCopyOptions {...}, items);

// retrieve products that do not have duplicates by Name
var query =
	from p in ctx.Products
	from op in ctx.Products.LeftJoin(op => op.ProductID != p.ProductID && op.Name == p.Name)
	where Sql.ToNullable(op.ProductID) == null
	select p;

// insert these records into the same or another table
query.Insert(ctx.Products.ToLinqToDBTable(), s => new Product { Name = s.Name ... });

// update these records by changing name based on previous value
query.Update(prev => new Product { Name = "U_" + prev.Name ... });

// delete records that matched by query
query.Delete();
```

Some extensions require LINQ To DB `ITable<T>` interface, which could be acquired from  `DbSet<T>` using `ToLinqToDBTable()` extension method. 

For `ITable<T>` interface LINQ To DB provides several extensions that may be useful for complex databases and custom queries:

```cs
table = table.TableName("NewTableName");     // change table name in query
table = table.DatabaseName("OtherDatabase"); // change database name, useful for cross database queries.
table = table.OwnerName("OtherOwner");       // change owner.

// inserting into other existing table Products2
query.Insert(ctx.Products.ToLinqToDBTable().TableName("Products2"), s => new Product { Name = s.Name ... });
```

It is not required to work directly with `LINQ To DB` `DataConnection` class but there are several ways to do that. `LINQ To DB` will try to reuse your configuration and select appropriate data provider:

```cs
// uing DbContext
using (var dc = ctx.CreateLinqToDbConnection())
{
   // linq queries using linq2db extensions
}

// using DbContextOptions
using (var dc = options.CreateLinqToDbConnection())
{
   // linq queries using linq2db extensions
}
```

You can use all `LINQ To DB` extension functions in your EF linq queries. Just ensure you have called `ToLinqToDB()` function before materializing objects for synchronous methods.

Since EF Core have defined it's own asynchronous methods, we have to duplicate them to avoid naming collisions. 
Async methods have the same name but tith `LinqToDB` suffix. E.g. `ToListAsyncLinqToDB()`, `SumAsyncLinqToDB()`, ect.

```cs
using (var ctx = CreateAdventureWorksContext())
{
	var productsWithModelCount =
		from p in ctx.Products
		select new
		{
			// Window Function
			Count = Sql.Ext.Count().Over().PartitionBy(p.ProductModelID).ToValue(),
			Product = p
		};

	var neededRecords =
		from p in productsWithModelCount
		where p.Count.Between(2, 4) // LINQ To DB extension
		select new
		{
			p.Product.Name,
			p.Product.Color,
			p.Product.Size,
			// retrieving value from column dynamically
			PhotoFileName = Sql.Property<string>(p.Product, "ThumbnailPhotoFileName")
		};

	// ensure we have replaced EF context
	var items1 = neededRecords.ToLinqToDB().ToArray();       
	
	// we have to call our method to avoid naming collisions
	var items2 = await neededRecords.ToArrayAsyncLinqToDB(); 
	
	// and simple bonus - how to generate SQL
	var sql = neededRecords.ToLinqToDB().ToString();
}
```

Also check [existing tests](https://github.com/linq2db/linq2db.EntityFrameworkCore/blob/master/Tests/LinqToDB.EntityFrameworkCore.Tests/ToolsTests.cs) in test project for some examples.

# Why should I want to use it?

There are many reasons. Some of them:

- you want to use advanced SQL functionality, not supported or poorly supported by EntityFrameworkCore like BulkCopy support, SQL MERGE operations, convinient DML (Insert/Delete/Update) operations and many-many-many other features LINQ To DB provides, but you need change tracking functionality that EntityFramework provides.
- you want to migrate to LINQ To DB, but need to do it step-by-step.
- just because LINQ To DB is cool.

# Current status

Right now it is an early preview. Below is a list of providers, that should work right now:

- SQL Server
- MySQL (including Devart and Pomelo providers)
- PostgreSQL (Both npgsql and Devart providers)
- SQLite (including Devart provider)
- Firebird
- DB2 LUW
- Oracle
- SQL Server CE

# Know limitations
- No Eager loading
- No Lazy loading
- `Include` function not supported
- Not yet supported Global Query Filters

# Help! It doesn't work!

If you encounter any issue with this library, first check issues to see if it was already reported and if not, feel free to report new issue.
