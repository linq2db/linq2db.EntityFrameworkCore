using System;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{
	public static partial class LinqToDBForEFTools
	{
		public static ITable<T> ToLinqToDBTable<T>(this DbSet<T> dbSet) 
			where T : class
		{
			var context = Implementation.GetCurrentContext(dbSet);
			if (context == null)
				throw new LinqToDBForEFToolsException("Can not evaluate current context from query");

			var dc = CreateLinqToDbContext(context);
			return dc.GetTable<T>();
		}

		public static ITable<T> ToLinqToDBTable<T>(this DbSet<T> dbSet, IDataContext dataContext) 
			where T : class
		{
			return dataContext.GetTable<T>();
		}

	}
}
