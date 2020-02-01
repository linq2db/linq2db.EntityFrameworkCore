﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.Tests.ValueConversion
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
			public virtual DbSet<SubDivision> Subdivisions { get; set; }
		}

		public ConvertorTests()
		{
			var optionsBuilder = new DbContextOptionsBuilder<ConvertorContext>();

			optionsBuilder
				.ReplaceService<IValueConverterSelector, IdValueConverterSelector>()
				.UseSqlServer("Server=.;Database=ConverterTests;Integrated Security=SSPI")
				.UseLoggerFactory(TestUtils.LoggerFactory);;

			_options = optionsBuilder.Options;
		}


		[Test]
		public void TestToList()
		{
			using (var ctx = new ConvertorContext(_options))
			using (var db = ctx.CreateLinqToDbConnection())
			{
				ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();


				var resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C1", Id = new Id<SubDivision, long>(0), Name = "N1", PermanentId = Guid.NewGuid() });

				resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C2", Id = new Id<SubDivision, long>(1), Name = "N2", PermanentId = Guid.NewGuid() });

				resut = db.InsertWithInt64Identity(new SubDivision()
					{ Code = "C3", Id = new Id<SubDivision, long>(2), Name = "N3", PermanentId = Guid.NewGuid() });

				
				db.GetTable<SubDivision>()
					.Where(s => s.Id == 1)
					.Set(s => s.ParentId, new Id<SubDivision, long>(11))
					.Update();

				db.GetTable<SubDivision>()
					.Where(s => s.Id == 2)
					.Set(s => s.ParentId, () => new Id<SubDivision, long>(33))
					.Update();

				var ef   = ctx.Subdivisions.Where(s => s.Id == 1L).ToArray();
				var ltdb = ctx.Subdivisions.ToLinqToDB().Where(s => s.Id == 1L).ToArray();
				var all  = ctx.Subdivisions.ToLinqToDB().ToArray();
			}
		}

	}
}
