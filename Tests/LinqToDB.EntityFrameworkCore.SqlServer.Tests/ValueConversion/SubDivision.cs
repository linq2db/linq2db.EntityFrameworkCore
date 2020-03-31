using System;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.ValueConversion
{
	public class SubDivision : IEntity<long>
	{
		long IEntity<long>.Id => Id;
		public Id<SubDivision, long> Id { get; set; }
		public Guid PermanentId { get; set; }
		public Id<SubDivision, long>? ParentId { get; set; }
		// public Id<LegalEntity, long> LegalEntityId { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public bool? IsDeleted { get; set; }
	}
}
