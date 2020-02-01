using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LinqToDB.EntityFrameworkCore.Tests.ValueConversion
{
	public sealed class IdValueConverter<TKey, TEntity> : ValueConverter<Id<TEntity, TKey>, TKey>
		where TEntity : IEntity<TKey>
	{
		public IdValueConverter(ConverterMappingHints mappingHints = null)
			: base(id => id.Value, key => new Id<TEntity, TKey>(key)) { }
	}
}
