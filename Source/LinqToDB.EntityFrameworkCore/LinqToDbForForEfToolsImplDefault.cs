using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
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

	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	/// <summary>
	/// Default EF.Core - LINQ To DB integration bridge implementation.
	/// </summary>
	[PublicAPI]
	public class LinqToDbForForEfToolsImplDefault : ILinqToDbForEfTools
	{
		/// <summary>
		/// Returns LINQ To DB provider, based on provider data from EF.Core.
		/// Could be overriden if you have issues with default detection mechanisms.
		/// </summary>
		/// <param name="providerInfo">Provider information, extracted from EF.Core.</param>
		/// <returns>LINQ TO DB provider instance.</returns>
		public virtual IDataProvider GetDataProvider(EfProviderInfo providerInfo)
		{
			//TODO:
			return new SqlServerDataProvider(ProviderName.SqlServer2008, SqlServerVersion.v2008);
		}

		/// <summary>
		/// Creates metadata provider for specified EF.Core data model. Default implementation use
		/// <see cref="EfCoreMetadataReader"/> metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <returns>LINQ To DB metadata provider for specified EF.Core model. Can return <c>null</c>.</returns>
		public virtual IMetadataReader CreateMetadataReader(IModel model)
		{
			return new EfCoreMetadataReader(model);
		}

		/// <summary>
		/// Creates mapping schema using provided EF.Core data model and metadata provider.
		/// </summary>
		/// <param name="model">EF.Core data model.</param>
		/// <param name="metadataReader">Additional optional LINQ To DB database metadata provider.</param>
		/// <returns>Mapping schema for provided EF.Core model.</returns>
		public virtual MappingSchema GetMappingSchema(IModel model, IMetadataReader metadataReader)
		{
			var reader = CreateMetadataReader(model);
			if (reader == null)
				return null;

			var schema = new MappingSchema();
			schema.AddMetadataReader(reader);
			// TODO: add provided reader to schema
			return schema;
		}

		/// <summary>
		/// Returns EF.Core <see cref="DbContextOptions"/> for specific <see cref="DbContext"/> instance.
		/// </summary>
		/// <param name="context">EF.Core <see cref="DbContext"/> instance.</param>
		/// <returns><see cref="DbContextOptions"/> instance.</returns>
		public virtual DbContextOptions GetContextOptions(DbContext context)
		{
			return null;
		}

		private static readonly MethodInfo GetTableMethodInfo =
			MemberHelper.MethodOf<IDataContext>(dc => dc.GetTable<object>()).GetGenericMethodDefinition();

		/// <summary>
		/// Transforms EF.Core expression tree to LINQ To DB expression.
		/// Method replaces EF.Core <see cref="EntityQueryable{TResult}"/> instances with LINQ To DB <see cref="DataExtensions.GetTable{T}(IDataContext)"/> calls.
		/// </summary>
		/// <param name="expression">EF.Core expression tree.</param>
		/// <param name="dc">LINQ To DB <see cref="IDataContext"/> instance.</param>
		/// <returns>Transformed expression.</returns>
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

		/// <summary>
		/// Extracts <see cref="DbContext"/> instance from <see cref="IQueryable"/> object.
		/// Due to unavailability of integration API in EF.Core this method use reflection and could broke after EF.Core update.
		/// </summary>
		/// <param name="query">EF.Core query.</param>
		/// <returns>Current <see cref="DbContext"/> instance.</returns>
		public virtual DbContext GetCurrentContext(IQueryable query)
		{
			var compilerField = typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);
			if (compilerField == null)
				throw new Exception($"Can not find private field '{typeof(EntityQueryProvider).GetType()}._queryCompiler' in EntityFrameworkCore assembly");

			var compiler = (QueryCompiler)compilerField.GetValue(query.Provider);

			var queryContextFactoryField = compiler.GetType().GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new Exception($"Can not find private field '{compiler.GetType()}._queryContextFactory' in EntityFrameworkCore assembly");

			var queryContextFactory = (RelationalQueryContextFactory)queryContextFactoryField.GetValue(compiler);
			var dependenciesProperty = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance);

			if (queryContextFactoryField == null)
				throw new Exception($"Can not find private property '{nameof(RelationalQueryContextFactory)}.Dependencies' in EntityFrameworkCore assembly");

			var dependencies = (QueryContextDependencies)dependenciesProperty.GetValue(queryContextFactory);

			return dependencies.CurrentDbContext?.Context;
		}

		/// <summary>
		/// Extracts EF.Core connection information object from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core connection data.</returns>
		public virtual EfConnectionInfo ExtractConnectionInfo(DbContextOptions options)
		{
			var relational = options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
			return new EfConnectionInfo
			{
				ConnectionString = relational?.ConnectionString,
				Connection = relational?.Connection
			};
		}

		/// <summary>
		/// Extracts EF.Core data model instance from <see cref="DbContextOptions"/>.
		/// </summary>
		/// <param name="options"><see cref="DbContextOptions"/> instance.</param>
		/// <returns>EF.Core data model instance.</returns>
		public virtual IModel ExtractModel(DbContextOptions options)
		{
			var coreOptions = options.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault();
			return coreOptions?.Model;
		}
	}
}
