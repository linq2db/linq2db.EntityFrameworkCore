module LinqToDB.EntityFrameworkCore.FSharpTests

open System
open System.Linq
open LinqToDB
open LinqToDB.Mapping
open LinqToDB.Data
open LinqToDB.EntityFrameworkCore.BaseTests
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open NUnit.Framework

[<CLIMutable>]
type WithIdentity = {
    [<Key>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    Id : int
    Name : string
}

let insert(context : IDataContext) a : int =
    context.InsertWithInt32Identity(a)
let update(dbset : LinqToDB.Linq.IUpdatable<_>) : int =
    dbset.Update()

type AppDbContext(options: DbContextOptions<AppDbContext>) =
    inherit DbContext(options)

    [<DefaultValue>] val mutable WithIdentity : DbSet<WithIdentity>
    member this.CompaniesInformation
        with get() = this.WithIdentity
        and set v = this.WithIdentity <- v

    override _.OnModelCreating builder = builder.RegisterOptionTypes()

    member this.createRecord(name : string) : int =
        let record : WithIdentity = {
            Id = -1
            Name = name
        }

        insert(this.CreateLinqToDbContext())(record)

    member this.updateRecord(id : int) (name : string) : int =
        this
            .CompaniesInformation
            .Where(fun d -> d.Id = id).ToLinqToDB()
            .Set((fun d -> d.Name), name)
        |> update

type TestDbContextFactory() =
    member this.CreateDbContext() =
        let options = new DbContextOptionsBuilder<AppDbContext>()
        options.UseLoggerFactory(TestUtils.LoggerFactory) |> ignore
        options.UseSqlite("DataSource=:memory:").UseFSharpTypes() |> ignore
        let context = new AppDbContext(options.Options)
        context.Database.OpenConnection()
        context.Database.EnsureCreated() |> ignore
        context

[<TestFixture>]
type Tests() =

    [<Test>]
    member this.TestLeftJoin() =
        let context = TestDbContextFactory().CreateDbContext()
        let q =
            context
                .WithIdentity
                .Join(
                    context.WithIdentity,
                    (fun p -> p.Id),
                    (fun c -> c.Id),
                    (fun p c ->
                    {|
                        Person = p
                        Company = c
                    |}) )
                .LeftJoin(
                    context.WithIdentity,
                    (fun partialPerson cInfo -> partialPerson.Company.Id = cInfo.Id),
                    (fun partialPerson cInfo ->
                    {|
                        Company = partialPerson.Company
                        CompanyInformation = cInfo
                        Person = partialPerson.Person
                    |}) )
        //q.ToArray() |> ignore
        q.ToLinqToDB().ToString() |> ignore

    [<Test>]
    member this.TestUpdate() =
        let context = TestDbContextFactory().CreateDbContext()

        let id = context.createRecord("initial name")

        let inserted = query {
            for p in context.CompaniesInformation do
            where (p.Id = id)
            exactlyOne }

        Assert.AreEqual("initial name", inserted.Name)

        let cnt = context.updateRecord(id)("new name")

        Assert.AreEqual(1, cnt)

        let readByLinqToDB = context.CompaniesInformation.Where(fun d -> d.Id = id).ToLinqToDB().ToArray()

        Assert.AreEqual(1, readByLinqToDB.Length)
        Assert.AreEqual("new name", readByLinqToDB[0].Name)

        let updated = query {
            for p in context.CompaniesInformation do
            where (p.Id = id)
            exactlyOne }

        Assert.AreEqual("new name", updated.Name)

