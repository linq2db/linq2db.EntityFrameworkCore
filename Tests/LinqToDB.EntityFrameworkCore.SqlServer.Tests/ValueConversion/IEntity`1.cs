namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.ValueConversion
{
	public interface IEntity<TKey>
	{
		public TKey Id { get; }
	}

}
