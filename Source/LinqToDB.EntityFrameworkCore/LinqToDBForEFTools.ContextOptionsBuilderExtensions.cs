using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LinqToDB.EntityFrameworkCore
{
	using Internal;

	public static partial class LinqToDBForEFTools
	{
		/// <summary>
		/// Registers custom options related to LinqToDB provider
		/// </summary>
		/// <param name="optionsBuilder"></param>
		/// <param name="linq2DbOptionsAction">Custom options action</param>
		/// <returns></returns>
		public static DbContextOptionsBuilder UseLinqToDB(
			this DbContextOptionsBuilder optionsBuilder,
			Func<DbContextOptionsBuilder, DataOptions, DataOptions>? linq2DbOptionsAction = null)
		{
			LinqToDBOptionsExtension linq2dbOptions;

			((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(linq2dbOptions = GetOrCreateExtension(optionsBuilder));

			if (linq2DbOptionsAction != null)
				linq2dbOptions.Options = linq2DbOptionsAction(optionsBuilder, linq2dbOptions.Options);

			return optionsBuilder;
		}

		private static LinqToDBOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder options)
			=> options.Options.FindExtension<LinqToDBOptionsExtension>()
				?? new LinqToDBOptionsExtension();
	}
}
