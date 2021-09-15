using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	public class TestUtils
	{
		public static readonly ILoggerFactory LoggerFactory =
			new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });
	}
}
