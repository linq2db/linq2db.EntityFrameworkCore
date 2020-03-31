using System.Collections.Generic;
using Northwind.Core.Domain.Entities;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind
{
    public partial class Region
    {
        public Region()
        {
            Territories = new HashSet<Territory>();
        }

        public int RegionId { get; set; }
        public string RegionDescription { get; set; }

        public ICollection<Territory> Territories { get; set; }
    }
}
