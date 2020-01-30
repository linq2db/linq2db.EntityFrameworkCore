using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;

namespace LinqToDB.EntityFrameworkCore.Tests.Models.AdventuresWorks
{
	public class AdventureWorksContextDerived : AdventureWorksContext
	{
		public AdventureWorksContextDerived(DbContextOptions options) : base(options)
		{
		}
	}
}
