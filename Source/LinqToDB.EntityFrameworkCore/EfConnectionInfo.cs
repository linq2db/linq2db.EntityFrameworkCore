using System.Data.Common;

namespace LinqToDB.EntityFrameworkCore
{
	public class EfConnectionInfo
	{
		public DbConnection Connection{ get; set; }
		public string ConnectionString{ get; set; }
	}
}