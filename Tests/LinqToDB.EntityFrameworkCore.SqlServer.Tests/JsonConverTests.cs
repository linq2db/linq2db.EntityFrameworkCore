using System;
using System.Linq;
using LinqToDB.EntityFrameworkCore.BaseTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	[TestFixture]
	public class JsonConverTests : TestsBase
	{
		private DbContextOptions<JsonConvertContext> _options;

		public class LocalizedString
		{
			public string English { get; set; } = null!;
			public string German { get; set; } = null!;
			public string Slovak { get; set; } = null!;
		}

		public class EventScheduleItemBase
		{
			public int Id { get; set; }
			public virtual LocalizedString NameLocalized { get; set; } = null!;
			public virtual string? JsonColumn { get; set; }
		}
		
		public enum CrashEnum : byte
		{
			OneValue = 0,
			OtherValue = 1
		}

		public class EventScheduleItem : EventScheduleItemBase
		{
			public CrashEnum CrashEnum { get; set; }
			public Guid GuidColumn { get; set; }
		}

		public class JsonConvertContext : DbContext
		{
			public JsonConvertContext()
			{
			}

			public JsonConvertContext(DbContextOptions<JsonConvertContext> options)
				: base(options)
			{
			}


			public virtual DbSet<EventScheduleItem> EventScheduleItems { get; set; } = null!;


			protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			{
				if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer("conn string");
			}

			protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
				modelBuilder.Entity<EventScheduleItem>(entity =>
				{
					entity.ToTable("EventScheduleItem");
					entity.Property(e => e.NameLocalized)
						.HasColumnName("NameLocalized_JSON")
						.HasConversion(v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<LocalizedString>(v) ?? new());
					entity.Property(e => e.CrashEnum).HasColumnType("tinyint");
					entity.Property(e => e.GuidColumn).HasColumnType("uniqueidentifier");
				});

				modelBuilder.HasDbFunction(typeof(JsonConverTests).GetMethod(nameof(JsonConverTests.JsonValue))!)
					.HasTranslation(e => new SqlFunctionExpression(
						"JSON_VALUE", e, true, e.Select(_ => false), typeof(string), null));
			}
		}

		public JsonConverTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<JsonConvertContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer(Settings.JsonConvertConnectionString);
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		public static string JsonValue(string? column, [NotParameterized] string path)
		{
			throw new NotSupportedException();
		}

		[Test]
		public void TestJsonConvert()
		{
			LinqToDBForEFTools.Initialize();
			
			// // converting from string, because usually JSON is stored as string, but it depends on DataProvider
			// Mapping.MappingSchema.Default.SetConverter<string, LocalizedString>(v => JsonConvert.DeserializeObject<LocalizedString>(v));
			//
			// // here we told linq2db how to pass converted value as DataParameter.
			// Mapping.MappingSchema.Default.SetConverter<LocalizedString, DataParameter>(v => new DataParameter("", JsonConvert.SerializeObject(v), LinqToDB.DataType.NVarChar));

			using (var ctx = new JsonConvertContext(_options))
			{
				ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();

				ctx.EventScheduleItems.Delete();

				ctx.EventScheduleItems.Add(new EventScheduleItem()
				{
					NameLocalized = new LocalizedString() { English = "English", German = "German", Slovak = "Slovak" },
					GuidColumn = Guid.NewGuid()
				});
				ctx.SaveChanges();

				var queryable = ctx.EventScheduleItems
					.Where(p => p.Id < 10).ToLinqToDB();

				var path = "some";

				var items = queryable
					.Select(p => new
					{
						p.Id,
						p.NameLocalized,
						p.CrashEnum,
						p.GuidColumn,
						JsonValue = JsonValue(p.JsonColumn, path)
					});

				var item = items.FirstOrDefault();

				Assert.IsNotNull(item);
				Assert.That(item!.NameLocalized.English, Is.EqualTo("English"));
				Assert.That(item.NameLocalized.German,   Is.EqualTo("German"));
				Assert.That(item.NameLocalized.Slovak,   Is.EqualTo("Slovak"));

				//TODO: make it work
				// var concrete = queryable.Select(p => new
				// {
				// 	p.Id,
				// 	English = p.NameLocalized.English
				// }).FirstOrDefault();
				//
				// Assert.That(concrete.English, Is.EqualTo("English"));
			}
		}
	}
}
