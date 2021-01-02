using LinqToDB.EntityFrameworkCore.BaseTests.Logging;
using Microsoft.Extensions.Logging;

namespace LinqToDB.EntityFrameworkCore.BaseTests
{
	public class TestUtils
	{
		public static readonly ILoggerFactory LoggerFactory =
			Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
			{
				builder.AddFilter("Microsoft", LogLevel.Information)
					.AddFilter("System", LogLevel.Warning)
					.AddFilter("LinqToDB.EntityFrameworkCore.Test", LogLevel.Information)

					.AddTestLogger(o =>
					{
						o.IncludeScopes = true;
					});
			});		

	}
}
