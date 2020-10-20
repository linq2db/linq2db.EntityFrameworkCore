namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
    public interface IHasId<T, TId> where T: IHasId<T, TId>
    {
        Id<T, TId> Id { get; }
    }
    
    public interface IHasWriteableId<T, TId> : IHasId<T, TId> where T: IHasWriteableId<T, TId>
    {
        new Id<T, TId> Id { get; set; }
    }
}
