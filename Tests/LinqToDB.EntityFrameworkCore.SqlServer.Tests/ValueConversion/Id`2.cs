using System;
using System.Collections.Generic;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.ValueConversion
{
	public readonly struct Id<TEntity, TKey> : IEquatable<Id<TEntity, TKey>> where TEntity : IEntity<TKey>
	{
		public Id(TKey id) => Value = id;
		public TKey Value { get; }
		public static implicit operator TKey(Id<TEntity, TKey> id) => id.Value;
		public override string ToString() => $"{typeof(TEntity).Name}({Value})";
		public bool Equals(Id<TEntity, TKey> other) => EqualityComparer<TKey>.Default.Equals(Value, other.Value);
		public override bool Equals(object obj) => obj is Id<TEntity, TKey> other && Equals(other);
		public override int GetHashCode() => EqualityComparer<TKey>.Default.GetHashCode(Value);
	}
}
