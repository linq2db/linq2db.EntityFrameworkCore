using System.Text.RegularExpressions;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public static partial class StringExtensions
	{
		public static string ToSnakeCase(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var startUnderscores = UnderscoresMatcher().Match(input);
#pragma warning disable CA1308 // Normalize strings to uppercase
			return startUnderscores + Replacer().Replace(input, "$1_$2").ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
		}

		[GeneratedRegex("^_+")]
		private static partial Regex UnderscoresMatcher();
		[GeneratedRegex("([a-z0-9])([A-Z])")]
		private static partial Regex Replacer();
	}
}
