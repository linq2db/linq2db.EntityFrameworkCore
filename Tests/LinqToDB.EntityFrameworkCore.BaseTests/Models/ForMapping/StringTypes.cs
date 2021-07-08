using System.ComponentModel.DataAnnotations;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping
{
	public class StringTypes
	{
		[Key]
		public int Id { get; set; }

		public string? AnsiString { get; set; }

		public string? UnicodeString { get; set; }
	}
}
