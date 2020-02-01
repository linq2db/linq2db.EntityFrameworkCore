using System;
using System.Collections.Generic;

namespace LinqToDB.EntityFrameworkCore.Tests.ValueConversion
{
	public interface IEntity<TKey>
	{
		public TKey Id { get; }
	}

}
