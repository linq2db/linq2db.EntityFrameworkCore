namespace LinqToDB.EntityFrameworkCore
{
	public class LinqToDBProviderInfo
	{
		public string Version { get; set; }
		public string ProviderName{ get; set; }

		public void Merge(LinqToDBProviderInfo provInfo)
		{
			if (provInfo != null)
			{
				Version = Version ?? provInfo.Version;
				ProviderName = ProviderName ?? provInfo.ProviderName;
			}
		}
	}
}
