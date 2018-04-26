using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{
	using DataProvider;
	using Mapping;
	using Metadata;

	public interface ILinqToDbForEfTools
	{
		/// <summary>
		/// Detects Linq2Db provider based on EintityFramework information. 
		/// </summary>
		/// <param name="providerInfo"></param>
		/// <returns></returns>
		IDataProvider GetDataProvider(EfProviderInfo providerInfo);

		/// <summary>
		/// Creates IMetadataReader implementation. 
		/// </summary>
		/// <param name="model"></param>
		/// <returns>IMetadataReader implemantetion. Can be null.</returns>
		IMetadataReader CreateMetadataReader(IModel model);

		/// <summary>
		/// Returns prepared MappingSchema.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="metadataReader"></param>
		/// <returns>Mapping schema for Model</returns>
		MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader);

		/// <summary>
		/// Implementation of retrieving options from DbContext
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		DbContextOptions GetContextOptions(DbContext context);

		/// <summary>
		/// Realisation for IQueryable expression transformation
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="dc"></param>
		/// <returns>Transformed expression</returns>
		Expression TransformExpression(Expression expression, IDataContext dc);

		/// <summary>
		/// Returns DBContext from IQueryable
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		DbContext GetCurrentContext(IQueryable query);

		/// <summary>
		/// Returns connection string or connection from options
		/// </summary>
		/// <param name="options"></param>
		/// <returns>Connection string</returns>
		EfConnectionInfo ExtractConnectionInfo(DbContextOptions options);

		/// <summary>
		/// Returns Entity model from options
		/// </summary>
		/// <param name="options"></param>
		/// <returns>Connection string</returns>
		IModel ExtractModel(DbContextOptions options);
	}
}
