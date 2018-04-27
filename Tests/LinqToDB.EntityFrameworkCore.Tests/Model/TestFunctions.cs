using System;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.Tests.Model
{
	public class TestFunctions
	{
		[DbFunction]
		public static DateTime GetDate()
		{
			return DateTime.Now;
		}

		[DbFunction]
		public static int Len(string value)
		{
			throw new NotImplementedException();
		}

	}
}
