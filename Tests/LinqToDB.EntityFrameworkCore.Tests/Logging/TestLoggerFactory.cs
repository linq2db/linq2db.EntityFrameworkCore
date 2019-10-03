using Microsoft.Extensions.Logging;

namespace LinqToDB.EntityFrameworkCore.Tests
{
	public class TestLoggerFactory: ILoggerFactory
	{
		public void Dispose()
		{
			
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new TestLogger2();
		}

		public void AddProvider(ILoggerProvider provider)
		{
		}
	}
}
