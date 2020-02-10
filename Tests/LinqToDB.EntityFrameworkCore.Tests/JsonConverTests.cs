using System.Linq;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.Tests
{

	[TestFixture]
	public class JsonConverTests : TestsBase
	{
		private DbContextOptions<JsonConvertContext> _options;

		public class LocalizedString
		{
			public string English { get; set; }
			public string German { get; set; }
			public string Slovak { get; set; }
		}

		public class EventScheduleItemBase
		{
			public int Id { get; set; }
			public virtual LocalizedString NameLocalized { get; set; }
		}

		public class EventScheduleItem : EventScheduleItemBase
		{
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


			public virtual DbSet<EventScheduleItem> EventScheduleItems { get; set; }


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
							v => JsonConvert.DeserializeObject<LocalizedString>(v));
				});
			}
		}

		public JsonConverTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<JsonConvertContext>();
			//new SqlServerDbContextOptionsBuilder(optionsBuilder);

			optionsBuilder.UseSqlServer("Server=.;Database=JsonConvertContext;Integrated Security=SSPI");
			optionsBuilder.UseLoggerFactory(TestUtils.LoggerFactory);

			_options = optionsBuilder.Options;
		}

		[Test]
		public void TestJsonConvert()
		{
			LinqToDBForEFTools.Initialize();
			
			// converting from string, because usually JSON is stored as string, but it depends on DataProvider
			Mapping.MappingSchema.Default.SetConverter<string, LocalizedString>(v => JsonConvert.DeserializeObject<LocalizedString>(v));

			// here we told linq2db how to pass converted value as DataParameter.
			Mapping.MappingSchema.Default.SetConverter<LocalizedString, DataParameter>(v => new DataParameter("", JsonConvert.SerializeObject(v), LinqToDB.DataType.NVarChar));

			using (var ctx = new JsonConvertContext(_options))
			{
				ctx.Database.EnsureCreated();

				ctx.EventScheduleItems.Delete();

				ctx.EventScheduleItems.Add(new EventScheduleItem()
				{
					NameLocalized = new LocalizedString() { English = "English", German = "German", Slovak = "Slovak" }
				});
				ctx.SaveChanges();

				var queryable = ctx.EventScheduleItems
					.Where(p => p.Id < 10).ToLinqToDB();

				var item = queryable
					.Select(p => new
					{
						p.Id,
						p.NameLocalized
					}).FirstOrDefault();
				
				Assert.That(item.NameLocalized.English, Is.EqualTo("English"));
				Assert.That(item.NameLocalized.German,  Is.EqualTo("German"));
				Assert.That(item.NameLocalized.Slovak,  Is.EqualTo("Slovak"));

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
