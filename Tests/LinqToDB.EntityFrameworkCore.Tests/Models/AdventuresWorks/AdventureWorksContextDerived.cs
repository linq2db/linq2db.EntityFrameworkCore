using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.Tests.Models.AdventuresWorks
{
	public class AdventureWorksContextDerived : AdventureWorksContext
	{
		public AdventureWorksContextDerived(DbContextOptions options) : base(options)
		{
		}
	}
}
