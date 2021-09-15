using System;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Logging
{
	internal class NullScope : IDisposable
	{
		public static NullScope Instance { get; } = new NullScope();

		private NullScope()
		{
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
}
