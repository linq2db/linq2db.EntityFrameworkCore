// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
	[Table("Product", Schema = "SalesLT")]
	public class Product
	{
		public Product()
		{
			OrderDetails = new HashSet<SalesOrderDetail>();
		}

		public int ProductID { get; set; }

		[Required]
		public string Name { get; set; }

		[MaxLength(15)]
		public string Color { get; set; }

		public DateTime? DiscontinuedDate { get; set; }
		public decimal ListPrice { get; set; }
		public DateTime ModifiedDate { get; set; }
		public int? ProductCategoryID { get; set; }
		public int? ProductModelID { get; set; }

		[Required]
		[MaxLength(25)]
		public string ProductNumber { get; set; }

		public DateTime? SellEndDate { get; set; }
		public DateTime SellStartDate { get; set; }

		[MaxLength(5)]
		public string Size { get; set; }

		public decimal StandardCost { get; set; }
		public byte[] ThumbNailPhoto { get; set; }

		[MaxLength(50)]
		public string ThumbnailPhotoFileName { get; set; }

		public decimal? Weight { get; set; }

#pragma warning disable IDE1006 // Naming Styles
		public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

		[InverseProperty("Product")]
		public virtual ICollection<SalesOrderDetail> OrderDetails { get; set; }

		[ForeignKey("ProductCategoryID")]
		[InverseProperty("Product")]
		public virtual ProductCategory ProductCategory { get; set; }

		[ForeignKey("ProductModelID")]
		[InverseProperty("Product")]
		public virtual ProductModel ProductModel { get; set; }
	}
}
