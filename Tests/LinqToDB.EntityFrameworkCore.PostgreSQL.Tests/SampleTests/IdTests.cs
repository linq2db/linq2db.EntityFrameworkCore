using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using LinqToDB.Common.Logging;
using LinqToDB.EntityFrameworkCore.BaseTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	[TestFixture]
	public sealed class IdTests : IDisposable
	{
		public IdTests()
		{
			_efContext = new TestContext(
				new DbContextOptionsBuilder()
					.ReplaceService<IValueConverterSelector, IdValueConverterSelector>()
					.UseLoggerFactory(TestUtils.LoggerFactory)
					.EnableSensitiveDataLogging()
					.UseNpgsql("Server=DBHost;Port=5432;Database=IdTests;User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;")
					.Options);
			_efContext.Database.EnsureDeleted();
			_efContext.Database.EnsureCreated();
		}

		IDataContext CreateLinqToDbContext(TestContext testContext)
		{
			var result = testContext.CreateLinqToDbContext();
			result.GetTraceSwitch().Level = TraceLevel.Verbose;
			return result;
		}

		readonly TestContext _efContext;

		[Test]
		[Ignore("Incomplete.")]
		public void TestInsertWithoutTracker([Values("test insert")] string name) 
			=> _efContext
				.Arrange(c => CreateLinqToDbContext(c))
				.Act(c => c.Insert(new Entity { Name = name }))
				.Assert(id => _efContext.Entitites.Single(e => e.Id == id).Name.Should().Be(name));

		[Test]
		[Ignore("Incomplete.")]
		public void TestInsertWithoutNew([Values("test insert")] string name) 
			=> _efContext.Entitites
				.Arrange(e => e.ToLinqToDBTable())
				.Act(e => e.InsertWithInt64Identity(() => new Entity {Name = name}))
				.Assert(id => _efContext.Entitites.Single(e => e.Id == id).Name.Should().Be(name));

		[Test]
		[Ignore("Incomplete.")]
		public void TestInsertEfCore([Values("test insert ef")] string name) 
			=> _efContext
				.Arrange(c => c.Entitites.Add(new Entity {Name = "test insert ef"}))
				.Act(_ => _efContext.SaveChanges())
				.Assert(_ => _efContext.Entitites.Single().Name.Should().Be(name));

		[Test]
		[Ignore("Incomplete.")]
		public void TestIncludeDetails([Values] bool l2db, [Values] bool tracking)
			=> _efContext
				.Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
				.Act(c => c
					.Entitites
					.Where(e => e.Name == "Alpha")
					.Include(e => e.Details)
					.ThenInclude(d => d.Details)
					.Include(e => e.Children)
					.AsLinqToDb(l2db)
					.AsTracking(tracking)
					.ToArray())
				.Assert(e => e.First().Details.First().Details.Count().Should().Be(2));

		[Test]
		public void TestManyToManyIncludeTrackerPoison([Values] bool l2db)
			=> _efContext
				.Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
				.Act(c =>
				{
					var q = c.Entitites
						.Include(e => e.Items)
						.ThenInclude(x => x.Item);
					var f = q.AsLinqToDb(l2db).AsTracking().ToArray();
					var s = q.AsLinqToDb(!l2db).AsTracking().ToArray();
					return (First: f, Second: s);
				})
				.Assert(r => r.First[0].Items.Count().Should().Be(r.Second[0].Items.Count()));
		
		
		[Test]
		[Ignore("Incomplete.")]
		public void TestManyToManyInclude([Values] bool l2db, [Values] bool tracking)
			=> _efContext
				.Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
				.Act(c => c.Entitites
					.Include(e => e.Items)
					.ThenInclude(x => x.Item)
					.AsLinqToDb(l2db)
					.AsTracking(tracking)
					.ToArray())
				.Assert(m => m[0].Items.First().Item.Should().BeSameAs(m[1].Items.First().Item));

		[Test]
		[Ignore("Incomplete.")]
		public void TestMasterInclude([Values] bool l2db, [Values] bool tracking)
			=> _efContext
				.Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
				.Act(c => c
					.Details
					.Include(d => d.Master)
					.AsLinqToDb(l2db)
					.AsTracking(tracking)
					.ToArray())
				.Assert(m => m[0].Master.Should().BeSameAs(m[1].Master));

		[Test]
		[Ignore("Incomplete.")]
		public void TestMasterInclude2([Values] bool l2db, [Values] bool tracking)
			=> _efContext
				.Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
				.Act(c => c
					.Details
					.Include(d => d.Master)
					.AsTracking(tracking)
					.AsLinqToDb(l2db)
					.ToArray())
				.Assert(m => m[0].Master.Should().BeSameAs(m[1].Master));

		void InsertDefaults(IDataContext dataContext)
		{
			var a = dataContext.Insert(new Entity {Name = "Alpha"});
			var b = dataContext.Insert(new Entity {Name = "Bravo"});
			var d = dataContext.Insert(new Detail {Name = "First", MasterId = a});
			var r = dataContext.Insert(new Item {Name = "Red"});
			var g = dataContext.Insert(new Item {Name = "Green"});
			var w = dataContext.Insert(new Item {Name = "White"});

			dataContext.Insert(new Detail {Name = "Second", MasterId = a});
			dataContext.Insert(new SubDetail {Name = "Plus", MasterId = d});
			dataContext.Insert(new SubDetail {Name = "Minus", MasterId = d});
			dataContext.Insert(new Child {Name = "One", ParentId = a});
			dataContext.Insert(new Child {Name = "Two", ParentId = a});
			dataContext.Insert(new Child {Name = "Three", ParentId = a});
			dataContext.Insert(new Entity2Item {EntityId = a, ItemId = r});
			dataContext.Insert(new Entity2Item {EntityId = a, ItemId = g});
			dataContext.Insert(new Entity2Item {EntityId = b, ItemId = r});
			dataContext.Insert(new Entity2Item {EntityId = b, ItemId = w});
		}

		public class TestContext : DbContext
		{
			public TestContext(DbContextOptions options) : base(options) { }
			protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
				modelBuilder.Entity<Entity2Item>().HasKey(x => new { x.EntityId, x.ItemId});
				modelBuilder
					.UseSnakeCase()
					.UseIdAsKey()
					.UseOneIdSequence<long>("test", sn => $"nextval('{sn}')");
			}


			public DbSet<Entity> Entitites { get; set; } = null!;
			public DbSet<Detail> Details { get; set; } = null!;
			public DbSet<SubDetail> SubDetails { get; set; } = null!;
			public DbSet<Item> Items { get; set; } = null!;
			public DbSet<Child> Children { get; set; } = null!;
		}

		public void Dispose() => _efContext.Dispose();
	}
}
