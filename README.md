# linq2db.EntityFrameworkCore

`linq2db.EntityFrameworkCore` is an integration of LINQ To DB with existing EntityFrameworkCore projects. It was inspired by [this issue](https://github.com/aspnet/EntityFrameworkCore/issues/11657) in EF.Core repository.

# How to use

As it is an early preview, first you will need:
- latest build of `LINQ To DB` from [MyGet](https://www.myget.org/feed/linq2db/package/nuget/linq2db) or build it from [source code](https://github.com/linq2db/linq2db)
- download and build `linq2db.EntityFrameworkCore` from this repository

In your code you need to initialize integration using following call:
```cs
LinqToDbForEfTools.Initialize();
```

After that you can just call DbContext and IQueryable extension methods, provided by `LINQ To DB`.

Also check [existing tests](https://github.com/linq2db/linq2db.EntityFrameworkCore/blob/master/Tests/LinqToDB.EntityFrameworkCore.Tests/ToolsTests.cs) in test project for some examples.

# Why should I want to use it?

There are many reasons. Some of them:
- you want to use advanced SQL functionality, not supported or poorly supported by EntityFrameworkCore like BulkCopy support, SQL MERGE operations, convinient DML (Insert/Delete/Update) operations and many-many-many other features LINQ To DB provides, but you need change tracking functionality that EntityFramework provides.
- you want to migrate to LINQ To DB, but need to do it step-by-step.
- just because LINQ To DB is cool.

# Current status

Right now it is an early preview that supports only SQL Server provider. More providers support is coming soon.

# Help! It doesn't work!

If you encounter any issue with this library, first check issues to see if it was already reported and if not, feel free to report new issue.
