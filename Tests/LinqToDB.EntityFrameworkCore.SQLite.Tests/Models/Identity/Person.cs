using System;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Identity
{
	public class Person
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
	}
}
