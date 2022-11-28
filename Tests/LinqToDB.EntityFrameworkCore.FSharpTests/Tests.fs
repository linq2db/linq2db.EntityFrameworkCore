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

[<CLIMutable>]
type OptionTable = {
    [<Key>]
    Id : int
    ReferenceOption : string option
    ValueOption : int option
    NValueOption : Nullable<int> option
}

type AppDbContext(options: DbContextOptions<AppDbContext>) =
    inherit DbContext(options)

    [<DefaultValue>] val mutable WithIdentity : DbSet<WithIdentity>
    [<DefaultValue>] val mutable Options : DbSet<OptionTable>
    member this.CompaniesInformation
        with get() = this.WithIdentity
        and set v = this.WithIdentity <- v
    member this.OptionsTable
        with get() = this.Options
        and set v = this.Options <- v
    override _.OnModelCreating builder = builder.RegisterOptionTypes()

type TestDbContextFactory() =
    member this.CreateDbContext() =
        //MappingSchema.Default.SetConvertExpression<int option, DataParameter>((fun (x : int option) -> new DataParameter(null, if x.IsSome then x.Value :> Object else null)), false)
        MappingSchema.Default.SetConverter<int option, DataParameter>((fun (x : int option) -> new DataParameter(null, if x.IsSome then x.Value :> Object else null)))
        MappingSchema.Default.SetConverter<int, DataParameter>((fun (x : int) -> new DataParameter(null, x)))
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
    member this.TestOption() =
        let context = TestDbContextFactory().CreateDbContext()

        let p =
            { ReferenceOption = None
              Id = 1
              ValueOption = None
              NValueOption = None}

        context.CreateLinqToDbContext().Insert(p) |> ignore

        let p =
            { ReferenceOption = Some "test"
              Id = 2
              ValueOption = Some 3
              NValueOption = Some (Nullable 4) }

        context.CreateLinqToDbContext().Insert(p) |> ignore

        let inserted = query {
            for p in context.Options do
            where (p.Id = 1)
            exactlyOne }

        Assert.AreEqual(None, inserted.ReferenceOption)
        // this line fails
        Assert.AreEqual(None, inserted.ValueOption)
        Assert.AreEqual(None, inserted.NValueOption)

        let inserted = query {
            for p in context.Options do
            where (p.Id = 2)
            exactlyOne }

        Assert.AreEqual(Some "test", inserted.ReferenceOption)
        Assert.AreEqual(Some 3, inserted.ValueOption)
        Assert.AreEqual(Some (Nullable 4), inserted.NValueOption)
