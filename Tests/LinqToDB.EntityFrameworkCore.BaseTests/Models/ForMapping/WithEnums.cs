namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.ForMapping
{
	public enum StatusEnum
	{
		Pending = 0,
		Verified = 1,
		Completed = 2,
		Rejected = 3,
		Reviewed = 4
	}

	public class WithEnums
	{
		public int Id { get; set; }
		public StatusEnum Status { get; set; }
	}
}
