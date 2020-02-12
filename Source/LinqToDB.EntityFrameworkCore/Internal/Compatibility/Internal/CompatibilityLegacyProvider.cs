using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LinqToDB.EntityFrameworkCore.Internal.Compatibility.Internal
{
	internal class CompatibilityLegacyProvider : ICompatibilityProvider
	{
		public PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			return type.GetPropertyEx(propertyName);
		}

		public bool IsEnum(Type type)
		{
			return type.IsEnumEx();
		}

		public bool ShouldUseMicrosoftDataSqlServer()
		{
			return false;
		}

		public string GetSqlServerAssemblyName()
		{
			return "System.Data.SqlClient";
		}

		public SqlServerDataProvider CreateSqlServerDataProvider(string providerName, SqlServerVersion version)
		{
			return new SqlServerDataProvider(providerName, version);
		}
	}
}
