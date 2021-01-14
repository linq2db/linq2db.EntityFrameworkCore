using LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind;
using LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind.Mapping;
using Microsoft.EntityFrameworkCore;

namespace LinqToDB.EntityFrameworkCore.SqlServer.Tests.Models.Northwind
{
	public class NorthwindContext : DbContext
	{
		public DbSet<Category>             Categories           { get; set; } = null!;
		public DbSet<CustomerCustomerDemo> CustomerCustomerDemo { get; set; } = null!;
		public DbSet<CustomerDemographics> CustomerDemographics { get; set; } = null!;
		public DbSet<Customer>             Customers            { get; set; } = null!;
		public DbSet<Employee>             Employees            { get; set; } = null!;
		public DbSet<EmployeeTerritory>    EmployeeTerritories  { get; set; } = null!;
		public DbSet<OrderDetail>          OrderDetails         { get; set; } = null!;
		public DbSet<Order>                Orders               { get; set; } = null!;
		public DbSet<Product>              Products             { get; set; } = null!;
		public DbSet<Region>               Region               { get; set; } = null!;
		public DbSet<Shipper>              Shippers             { get; set; } = null!;
		public DbSet<Supplier>             Suppliers            { get; set; } = null!;
		public DbSet<Territory>            Territories          { get; set; } = null!;

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
