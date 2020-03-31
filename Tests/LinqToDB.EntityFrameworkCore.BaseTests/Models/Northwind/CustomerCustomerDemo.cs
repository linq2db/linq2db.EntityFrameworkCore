using Northwind.Core.Domain.Entities;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind
{
    public class CustomerCustomerDemo
    {
        public string CustomerId { get; set; }
        public string CustomerTypeId { get; set; }

        public Customer Customer { get; set; }
        public CustomerDemographics CustomerType { get; set; }
    }
}
