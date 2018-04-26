using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore
{
	public class EfProviderInfo
	{
		public string ServerVersion { get; set; }
		public DbConnection Connection { get; set; }
		public DbContext Context{ get; set; }
		public DbContextOptions Options{ get; set; }
	}
}
