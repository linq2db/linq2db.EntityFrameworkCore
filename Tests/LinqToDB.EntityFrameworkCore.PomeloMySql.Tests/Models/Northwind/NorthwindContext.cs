using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind.Mapping;
using Microsoft.EntityFrameworkCore;
using Northwind.Core.Domain;
using Northwind.Core.Domain.Entities;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind
{
	public class NorthwindContext : DbContext
	{
		public DbSet<Category> Categories { get; set; }
		public DbSet<CustomerCustomerDemo> CustomerCustomerDemo { get; set; }
		public DbSet<CustomerDemographics> CustomerDemographics { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<EmployeeTerritory> EmployeeTerritories { get; set; }
		public DbSet<OrderDetail> OrderDetails { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Region> Region { get; set; }
		public DbSet<Shipper> Shippers { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<Territory> Territories { get; set; }

		public NorthwindContext(DbContextOptions options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new CategoriesMap());
			builder.ApplyConfiguration(new CustomerCustomerDemoMap());
			builder.ApplyConfiguration(new CustomerDemographicsMap());
			builder.ApplyConfiguration(new CustomersMap());
			builder.ApplyConfiguration(new EmployeesMap());
			builder.ApplyConfiguration(new EmployeeTerritoriesMap());
			builder.ApplyConfiguration(new OrderDetailsMap());
			builder.ApplyConfiguration(new OrderMap());
			builder.ApplyConfiguration(new ProductsMap());
			builder.ApplyConfiguration(new RegionMap());
			builder.ApplyConfiguration(new ShippersMap());
			builder.ApplyConfiguration(new SuppliersMap());
			builder.ApplyConfiguration(new TerritoriesMap());

			builder.Entity<Product>()
				.HasQueryFilter(e => !IsFilterProducts || e.ProductId > 2);
		}

		public bool IsFilterProducts { get; set; } 
	}
}
