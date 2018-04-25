using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace LinqToDB.EntityFrameworkCore
{
	using DataProvider;
	using DataProvider.SqlServer;
	using Expressions;
	using Extensions;
	using Mapping;
	using Metadata;

	public class Linq2DbToolsImplDefault : ILinq2DbTools
	{
		/// <summary>
		/// Detects Linq2Db provider based on EintityFramework information. 
		/// Should be overriden if you have experienced problem in detecting specific provider. 
		/// </summary>
		/// <param name="providerInfo"></param>
		/// <returns></returns>
		public virtual IDataProvider GetDataProvider(EfProviderInfo providerInfo)
		{
			//TODO:
			return new SqlServerDataProvider(ProviderName.SqlServer2008, SqlServerVersion.v2008);
		}

		/// <summary>
		/// Creates IMetadataReader implementation. Can be overriden to specify own MetadaData reader
		/// </summary>
		/// <param name="model"></param>
		/// <returns>IMetadataReader implemantetion. Can be null.</returns>
		public virtual IMetadataReader CreateMetadataReader(IModel model)
		{
			return new EfCoreMetadataReader(model);
		}

		/// <summary>
		/// Default implemntation of creation mapping schema for model.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="metadataReader"></param>
		/// <returns>Mapping schema for Model</returns>
		public virtual MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader)
		{
			var reader = CreateMetadataReader(model);
			if (reader == null)
				return null;

			var schema = new MappingSchema();
			schema.AddMetadataReader(reader);
			return schema;
		}

		/// <summary>
		/// Default implementation of retrieving options from DbContext
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual DbContextOptions GetContextOptions(DbContext context)
		{
			return null;
		}

		public static readonly MethodInfo GetTableMethodInfo =
			MemberHelper.MethodOf<IDataContext>(dc => dc.GetTable<object>()).GetGenericMethodDefinition();


		/// <summary>
		/// Default realisation for IQueryable expression transformation
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="dc"></param>
		/// <returns>Transformed expression</returns>
		public virtual Expression TransformExpression(Expression expression, IDataContext dc)
		{
			var newExpression =
				expression.Transform(e =>
				{
					switch (e.NodeType)
					{
						case ExpressionType.Constant:
						{
							if (typeof(EntityQueryable<>).IsSameOrParentOf(e.Type))
							{
								var newExpr = Expression.Call(null,
									GetTableMethodInfo.MakeGenericMethod(e.Type.GenericTypeArguments),
									Expression.Constant(dc)
								);
								return newExpr;
							}

							break;
						}
					}

					return e;
				});

			return newExpression;
		}

		public virtual DbContext GetCurrentContext(IQueryable query)
		{
			var compilerField = typeof (EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);
			var compiler = (QueryCompiler) compilerField.GetValue(query.Provider);

			var queryContextFactoryField = compiler.GetType().GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);
			var queryContextFactory = (RelationalQueryContextFactory) queryContextFactoryField.GetValue(compiler);	    

			var dependenciesProperty = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance);
			var dependencies = (QueryContextDependencies) dependenciesProperty.GetValue(queryContextFactory);

			return dependencies.CurrentDbContext?.Context;
		}

		public virtual EfConnectionInfo ExtractConnectionInfo(DbContextOptions options)
		{
			var relational = options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			return new  EfConnectionInfo
			{
				ConnectionString = relational?.ConnectionString,
				Connection = relational?.Connection
			};
		}

		public virtual IModel ExtractModel(DbContextOptions options)
		{
			var coreOptions = options.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault();
			return coreOptions?.Model;
		}

	}
}