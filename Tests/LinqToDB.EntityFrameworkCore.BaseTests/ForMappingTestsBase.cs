using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.BaseTests
{
	public abstract class ForMappingTestsBase : TestsBase
	{
		public abstract ForMappingContextBase CreateContext();

		[Test]
		public virtual void TestIdentityMapping()
		{
			using var context = CreateContext();
			using var connection = context.CreateLinqToDbConnection();

			var ed = connection.MappingSchema.GetEntityDescriptor(typeof(WithIdentity));
			var pk = ed.Columns.Where(c => c.IsPrimaryKey).Single();

			pk.IsIdentity.Should().BeTrue();
		}

		[Test]
		public virtual void TestNoIdentityMapping()
		{
			using var context = CreateContext();
			using var connection = context.CreateLinqToDbConnection();

			var ed = connection.MappingSchema.GetEntityDescriptor(typeof(NoIdentity));
			var pk = ed.Columns.Where(c => c.IsPrimaryKey).Single();

			pk.IsIdentity.Should().BeFalse();
		}

		[Test]
		public virtual void TestTableCreation()
		{
			using var context = CreateContext();
			using var connection = context.CreateLinqToDbConnection();

			using var t1 = connection.CreateTempTable<WithIdentity>();
			using var t2 = connection.CreateTempTable<NoIdentity>();
		}


		[Test]
		public virtual void TestBulkCopyNoIdentity()
		{
			using var context = CreateContext();
			using var connection = context.CreateLinqToDbConnection();

			using var t = connection.CreateTempTable<NoIdentity>();

			var items = new List<NoIdentity>()
			{
				new() {Id = Guid.NewGuid(), Name = "John Doe"},
				new() {Id = Guid.NewGuid(), Name = "Jane Doe"}
			};

			t.BulkCopy(items);


			items.Should().BeEquivalentTo(t);
		}

		[Test]
		public virtual void TestBulkCopyWithIdentity()
		{
			using var context = CreateContext();
			using var connection = context.CreateLinqToDbConnection();

			using var t = connection.CreateTempTable<WithIdentity>();

			var items = new List<WithIdentity>()
			{
				new() {Id = 1, Name = "John Doe"},
				new() {Id = 2, Name = "Jane Doe"}
			};

			t.BulkCopy(items);


			t.Should().HaveCount(items.Count);
		}

		[Test]
		public virtual async Task TestUIntTable()
		{
			using var context = CreateContext();
			context.UIntTable.Add(new UIntTable
			{
				Field16 = 1,
				Field16N = 2,
				Field32 = 3,
				Field32N = 4,
				Field64 = 5,
				Field64N = 6
			});

			await context.SaveChangesAsync();

			ulong field64 = 5;
			var item = await context.UIntTable.FirstOrDefaultAsyncLinqToDB(e => e.Field64 == field64);
		}

	}
}
