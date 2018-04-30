using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore
{
	using DataProvider;
	using Mapping;
	using Metadata;

	public interface ILinqToDBForEFTools
	{
		/// <summary>
		/// Returns LINQ To DB provider, based on provider data from EF.Core.
		/// </summary>
		/// <param name="providerInfo"></param>
		/// <returns></returns>
		IDataProvider GetDataProvider(EFProviderInfo providerInfo);

		/// <summary>
		/// Creates metadata provider for specified EF.Core data model.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <returns>LINQ To DB metadata provider for specified EF.Core model. Can return <c>null</c>.</returns>
		IMetadataReader CreateMetadataReader(IModel model);

		/// <summary>
		/// Creates mapping schema using provided EF.Core data model and metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="metadataReader">Additional optional LINQ To DB database metadata provider.</param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader);

		/// <summary>
		/// Returns EF.Core <see cref="DbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="DbContextOptions"/> instance.</returns>
		DbContextOptions GetContextOptions(DbContext context);

		/// <summary>
		/// Transforms EF.Core expression tree to LINQ To DB expression.
		/// </summary>
		/// <param name="expression">EF.Core expression tree.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <returns>Transformed expression.</returns>
		Expression TransformExpression(Expression expression, IDataContext dc);

		/// <summary>
		/// Extracts <see cref="DbContext"/> instance from <see cref="IQueryable"/> object.
		/// </summary>
		/// <param name="query">EF.Core query.</param>
		/// <returns>Current <see cref="DbContext"/> instance.</returns>
		DbContext GetCurrentContext(IQueryable query);

		/// <summary>
		/// Extracts EF.Core connection information object from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"></param>
		/// <returns>Connection string</returns>
		EFConnectionInfo ExtractConnectionInfo(DbContextOptions options);

		/// <summary>
		/// Extracts EF.Core data model instance from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core data model instance.</returns>
		IModel ExtractModel(DbContextOptions options);
	}
}
