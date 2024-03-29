﻿namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
	public interface IHasId<T, TId>
		where T: IHasId<T, TId>
		where TId : notnull
	{
		Id<T, TId> Id { get; }
	}
	
	public interface IHasWriteableId<T, TId> : IHasId<T, TId>
		where T: IHasWriteableId<T, TId>
		where TId : notnull
	{
		new Id<T, TId> Id { get; set; }
	}
}
