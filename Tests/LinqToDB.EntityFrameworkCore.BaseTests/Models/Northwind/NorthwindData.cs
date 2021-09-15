// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind
{
	public partial class NorthwindData
	{
		private readonly Customer[] _customers;
		private readonly CustomerView[] _customerViews;
		private readonly Employee[] _employees;
		private readonly Product[] _products;
		private readonly Order[] _orders;
		private readonly OrderDetail[] _orderDetails;

		public NorthwindData()
		{
			_customers = CreateCustomers();
			_employees = CreateEmployees();
			_products = CreateProducts();
			_orders = CreateOrders();
			_orderDetails = CreateOrderDetails();

			var customerViews = new List<CustomerView>();

			foreach (var customer in _customers)
			{
				customer.Orders = new List<Order>();

				customerViews.Add(
					new CustomerView
					{
						Address = customer.Address,
						City = customer.City,
						CompanyName = customer.CompanyName,
						ContactName = customer.ContactName,
						ContactTitle = customer.ContactTitle
					});
			}

			_customerViews = customerViews.ToArray();

			foreach (var product in _products)
			{
				product.OrderDetails = new List<OrderDetail>();
			}

 
			foreach (var orderDetail in _orderDetails)
			{
				var order = _orders.First(o => o.OrderId == orderDetail.OrderId);
				orderDetail.Order = order;
				order.OrderDetails.Add(orderDetail);

				var product = _products.First(p => p.ProductId == orderDetail.ProductId);
				orderDetail.Product = product;
				product.OrderDetails.Add(orderDetail);
			}

			// foreach (var employee in _employees)
			// {
			//     var manager = _employees.FirstOrDefault(e => employee.ReportsTo == e.EmployeeId);
			//     employee.Manager = manager;
			// }
		}

		public static void Seed(DbContext context)
		{
			AddEntities(context);

			context.SaveChanges();
		}

		public static Task SeedAsync(DbContext context)
		{
			AddEntities(context);

			return context.SaveChangesAsync();
		}

		private static void AddEntities(DbContext context)
		{
			context.Set<Customer>().AddRange(CreateCustomers());

			var titleProperty = context.Model.FindEntityType(typeof(Employee)).FindProperty("Title");
			foreach (var employee in CreateEmployees())
			{
				context.Set<Employee>().Add(employee);
#pragma warning disable EF1001 // Internal EF Core API usage.
				context.Entry(employee).GetInfrastructure()[titleProperty] = employee.Title;
#pragma warning restore EF1001 // Internal EF Core API usage.
			}

			context.Set<Order>().AddRange(CreateOrders());
			context.Set<Category>().AddRange(CreateCategories());
			context.Set<Supplier>().AddRange(CreateSupliers());
			context.Set<Shipper>().AddRange(CreateShippers());
			context.Set<Product>().AddRange(CreateProducts());
			context.Set<OrderDetail>().AddRange(CreateOrderDetails());
		}
	}
}
