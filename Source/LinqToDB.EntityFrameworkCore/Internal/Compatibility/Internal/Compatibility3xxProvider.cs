using LinqToDB.Configuration;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LinqToDB.EntityFrameworkCore.Internal.Compatibility.Internal
{
	internal class Compatibility3xxProvider : ICompatibilityProvider
	{
		private readonly bool _isEFCore3x = typeof(DbContext).Assembly.GetName().Version.Major >= 3;
		private IDataProviderFactory _sqlServerFactory;

		public PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			return type.GetProperty(propertyName);
		}

		public bool IsEnum(Type type)
		{
			return type.IsEnum;
		}

		public string GetSqlServerAssemblyName()
		{
			return _isEFCore3x ? "Microsoft.Data.SqlClient" : "System.Data.SqlClient";
		}

		public SqlServerDataProvider CreateSqlServerDataProvider(string providerName, SqlServerVersion version)
		{
			return (SqlServerDataProvider)GetDataProviderFactory().GetDataProvider(new List<NamedValue>()
			{
				new NamedValue() { Name = "version", Value = version.ToString().Substring(1)},
				new NamedValue() { Name = "assemblyName", Value = GetSqlServerAssemblyName()},
			});
		}

		private IDataProviderFactory GetDataProviderFactory()
		{
			if (_sqlServerFactory == null)
			{
				_sqlServerFactory = (IDataProviderFactory)Activator.CreateInstance(typeof(IDataProviderFactory).Assembly.GetType("LinqToDB.DataProvider.SqlServer.SqlServerFactory"));
			}

			return _sqlServerFactory;
		}
	}
}
