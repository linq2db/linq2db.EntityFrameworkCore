using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.EntityFrameworkCore.Internal.Compatibility.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	/// <summary>
	/// Set of internal tools to allow running the library with 2xx as well as 3xx version
	/// </summary>
	internal static class LinqToDBCompatibilityTools
	{
		private static readonly ICompatibilityProvider _compatibilityProvider = CompatibilityProviderFactory.CreateInstance();

		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			return _compatibilityProvider.GetPropertyInfo(type, propertyName);
		}

		public static bool IsEnum(Type type)
		{
			return _compatibilityProvider.IsEnum(type);
		}

		public static string GetSqlServerAssemblyName()
		{
			return _compatibilityProvider.GetSqlServerAssemblyName();
		}

		public static SqlServerDataProvider CreateSqlServerDataProvider(string providerName, SqlServerVersion version)
		{
			return _compatibilityProvider.CreateSqlServerDataProvider(providerName, version);
		}
	}
}
