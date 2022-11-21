using System.Text.RegularExpressions;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public static partial class StringExtensions
	{
		public static string ToSnakeCase(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var startUnderscores = UnderscoresMatcher.Match(input);
			return startUnderscores + Replacer.Replace(input, "$1_$2").ToLowerInvariant();
		}

		// TODO: uncomment after azure pipelines updated to 17.4
		//[GeneratedRegex("^_+")]
		//private static partial Regex UnderscoresMatcher();
		//[GeneratedRegex("([a-z0-9])([A-Z])")]
		//private static partial Regex Replacer();

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
		private static readonly Regex UnderscoresMatcher = new ("^_+", RegexOptions.Compiled);
		private static readonly Regex Replacer = new ("([a-z0-9])([A-Z])", RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
	}
}
