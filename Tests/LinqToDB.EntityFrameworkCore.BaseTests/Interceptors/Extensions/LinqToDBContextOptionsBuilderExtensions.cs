using System.Linq;
using LinqToDB.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Interceptors.Extensions
{
	public static class LinqToDBContextOptionsBuilderExtensions
	{
		public static DataOptions UseEfCoreRegisteredInterceptorsIfPossible(this DbContextOptionsBuilder builder, DataOptions options)
		{
			var coreEfExtension = builder.Options.FindExtension<CoreOptionsExtension>();
			if (coreEfExtension?.Interceptors != null)
			{
				foreach (var comboInterceptor in coreEfExtension.Interceptors.OfType<IInterceptor>())
					options = options.UseInterceptor(comboInterceptor);
			}

			return options;
		}
	}
}
