using System;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
    public static class TypeExtensions
    {
        public static Type UnwrapNullable(this Type type) 
            => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;
    }
}
