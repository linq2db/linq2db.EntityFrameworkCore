using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LinqToDB.EntityFrameworkCore.BaseTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.ValueConversion
{
	[TestFixture]
	public class ConvertorTests
	{
		private DbContextOptions<ConvertorContext> _options;

		public class ConvertorContext : DbContext
		{
			public ConvertorContext(DbContextOptions options) : base(options)
			{
			}

			[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
			public virtual DbSet<SubDivision> Subdivisions { get; set; } = null!;
		}

		public ConvertorTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ConvertorContext>();

			optionsBuilder
				.ReplaceService<IValueConverterSelector, IdValueConverterSelector>()
				.UseSqlServer(Settings.ConverterConnectionString)
				.UseLoggerFactory(TestUtils.LoggerFactory);;

			_options = optionsBuilder.Options;
		}


		[Test]
		public void TestToList()
		{
			using (var ctx = new ConvertorContext(_options))
			using (var db = ctx.CreateLinqToDBConnection())
			{
				ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();


				var resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C1", Id = new Id<SubDivision, long>(0), Name = "N1", PermanentId = Guid.NewGuid() });

				resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C2", Id = new Id<SubDivision, long>(1), Name = "N2", PermanentId = Guid.NewGuid() });

				resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C3", Id = new Id<SubDivision, long>(2), Name = "N3", PermanentId = Guid.NewGuid() });
			
				var ef   = ctx.Subdivisions.Where(s => s.Id == 1L).ToArray();
				var ltdb = ctx.Subdivisions.ToLinqToDB().Where(s => s.Id == 1L).ToArray();
				
				var id = new Id<SubDivision, long>?(0L.AsId<SubDivision>());
				var ltdb2 = ctx.Subdivisions.ToLinqToDB().Where(s => s.Id == id).ToArray();
				
				var ids = new[] {1L.AsId<SubDivision>(), 2L.AsId<SubDivision>(),};
				var ltdbin = ctx.Subdivisions.ToLinqToDB()
					.Where(s => ids.Contains(s.Id)).ToArray();
				
				var all = ctx.Subdivisions.ToLinqToDB().ToArray();
				
				Assert.AreEqual(ef[0].Code, ltdb[0].Code);
				Assert.AreEqual(ef[0].Id, ltdb[0].Id);
				
				Assert.AreEqual(ef[0].Code, ltdb2[0].Code);
				Assert.AreEqual(ef[0].Id, ltdb2[0].Id);
			}
		}
	}
}
