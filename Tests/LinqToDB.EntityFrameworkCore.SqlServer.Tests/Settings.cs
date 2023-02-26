namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests
{
	public static class Settings
	{
		public static readonly string ForMappingConnectionString  = "Server=.;Database=ForMapping;Integrated Security=SSPI;Encrypt=true;TrustServerCertificate=true";
		public static readonly string IssuesConnectionString      = "Server=.;Database=IssuesEFCore;Integrated Security=SSPI;Encrypt=true;TrustServerCertificate=true";
		public static readonly string JsonConvertConnectionString = "Server=.;Database=JsonConvertContext;Integrated Security=SSPI;Encrypt=true;TrustServerCertificate=true";
		public static readonly string NorthwindConnectionString   = "Server=.;Database=NorthwindEFCore;Integrated Security=SSPI;Encrypt=true;TrustServerCertificate=true";
		public static readonly string ConverterConnectionString   = "Server=.;Database=ConverterTests;Integrated Security=SSPI;Encrypt=true;TrustServerCertificate=true";
	}
}
