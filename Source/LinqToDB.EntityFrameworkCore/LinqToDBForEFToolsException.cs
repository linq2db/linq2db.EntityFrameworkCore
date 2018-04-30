using System;

namespace LinqToDB.EntityFrameworkCore
{
	public class LinqToDBForEFToolsException : Exception
	{
		public LinqToDBForEFToolsException()
		{
		}

		public LinqToDBForEFToolsException(string message) : base(message)
		{
		}

		public LinqToDBForEFToolsException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
