using System.Collections.Generic;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.SampleTests
{
    public sealed class Detail : IHasWriteableId<Detail, long>
    {
        public Id<Detail, long> Id { get; set; }
        public Id<Entity, long> MasterId { get; set; }
        public string Name { get; set; }
        public Entity Master { get; set; }
        public IEnumerable<SubDetail> Details { get; set; }
    }
}
