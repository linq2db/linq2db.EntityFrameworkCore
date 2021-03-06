﻿using System;
using System.ComponentModel.DataAnnotations;

namespace LinqToDB.EntityFrameworkCore.PostgreSQL.Tests.Models.NpgSqlEntities
{
	public class EntityWithArrays
	{
		[Key]
		public int Id { get; set; }

		public Guid[] Guids { get; set; } = null!;
	}
}
