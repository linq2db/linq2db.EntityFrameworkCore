using System;
using System.Diagnostics.CodeAnalysis;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public static class TypeExtensions
	{
		[return: NotNullIfNotNull(nameof(type))]
		public static Type? UnwrapNullable(this Type? type)
			=> type == null ? null : Nullable.GetUnderlyingType(type) ?? type;
	}
}
