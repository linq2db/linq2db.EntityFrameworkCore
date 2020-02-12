using LinqToDB.DataProvider.SqlServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LinqToDB.EntityFrameworkCore.Internal.Compatibility.Internal
{
	internal interface ICompatibilityProvider
	{
		string GetSqlServerAssemblyName();
		SqlServerDataProvider CreateSqlServerDataProvider(string providerName, SqlServerVersion version);
		PropertyInfo GetPropertyInfo(Type type, string propertyName);
		bool IsEnum(Type type);
	}
}
