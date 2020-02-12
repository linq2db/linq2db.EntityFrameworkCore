using LinqToDB.DataProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.EntityFrameworkCore.Internal.Compatibility.Internal
{
	internal static class CompatibilityProviderFactory
	{
		public static ICompatibilityProvider CreateInstance()
		{
			if ((typeof(IDataProvider).Assembly.GetName().Version.Major >= 3))
			{
				return new Compatibility3xxProvider();
			}
			else
			{
				return new CompatibilityLegacyProvider();
			}
		}
	}
}
