using System;
using System.Collections.Generic;
using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;

namespace Northwind.Core.Domain.Entities
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
