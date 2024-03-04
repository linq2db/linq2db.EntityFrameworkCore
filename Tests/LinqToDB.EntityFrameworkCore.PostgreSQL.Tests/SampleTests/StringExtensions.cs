using System.Text.RegularExpressions;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public static class StringExtensions
	{
		public static string ToSnakeCase(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var startUnderscores = Regex.Match(input, @"^_+");
#pragma warning disable CA1308 // Normalize strings to uppercase
			return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
		}
	}
}
