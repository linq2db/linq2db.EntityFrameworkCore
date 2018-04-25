using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LinqToDB.EntityFrameworkCore
{
	using Mapping;
	using Metadata;

	class EfCoreMetadataReader : IMetadataReader
	{
		private readonly IModel _model;

		public EfCoreMetadataReader(IModel model)
		{
			_model = model;
		}

		public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
		{
			var et = _model?.FindEntityType(type);
			if (et != null)
			{
				if (typeof(T) == typeof(TableAttribute))
				{
					var relational = et.Relational();
					return new[] { (T) (Attribute) new TableAttribute(relational.TableName) { Schema = relational.Schema } };
				}
			}

			if (typeof(T) == typeof(TableAttribute))
			{
				var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>(inherit);
				if (tableAttribute != null)
					return new[] { (T) (Attribute) new TableAttribute(tableAttribute.Name) { Schema = tableAttribute.Schema } };
			}

			return Array.Empty<T>();
		}

		public T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute
		{
			if (typeof(T) == typeof(ColumnAttribute))
			{
				var et = _model?.FindEntityType(type);
				if (et != null)
				{
					var props = et.GetProperties();
					var prop = props.FirstOrDefault(p => p.GetIdentifyingMemberInfo() == memberInfo);
					if (prop != null)
					{
						var relational = prop.Relational();

						return new T[]{(T)(Attribute) new ColumnAttribute
						{
							Name = relational.ColumnName,
							Length = prop.GetMaxLength() ?? 0,
							CanBeNull = prop.IsNullable,
							DbType = relational.ColumnType
						}};
					}
				}

				var columnAttributes = memberInfo.GetCustomAttributes<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>(inherit);

				return columnAttributes.Select(c => (T) (Attribute) new ColumnAttribute
				{
					Name = c.Name,
					DbType = c.TypeName,
				}).ToArray();
			}

			if (typeof(T) == typeof(AssociationAttribute))
			{
				var et = _model?.FindEntityType(type);
				var navigations = et?.GetNavigations().Where(n => n.PropertyInfo == memberInfo).ToArray();
				if (navigations?.Length > 0)
				{
					var associations = new List<AssociationAttribute>();
					foreach (var navigation in navigations)
					{
						var fk = navigation.ForeignKey;
						if (fk.PrincipalEntityType == et)
						{
							var thisKey = string.Join(",", fk.PrincipalKey.Properties.Select(p => p.Name));
							var otherKey = string.Join(",", fk.Properties.Select(p => p.Name));
							associations.Add(new AssociationAttribute
							{
								ThisKey = thisKey,
								OtherKey = otherKey,
								CanBeNull = !fk.IsRequired,
								IsBackReference = false
							});
						}
						else
						{
							var thisKey = string.Join(",", fk.Properties.Select(p => p.Name));
							var otherKey = string.Join(",", fk.PrincipalKey.Properties.Select(p => p.Name));
							associations.Add(new AssociationAttribute
							{
								ThisKey = thisKey,
								OtherKey = otherKey,
								CanBeNull = !fk.IsRequired,
								IsBackReference = true
							});
						}
					}

					return associations.Select(a => (T) (Attribute) a).ToArray();
				}
			}

			return Array.Empty<T>();
		}

		public MemberInfo[] GetDynamicColumns(Type type)
		{
			return Array.Empty<MemberInfo>();
		}
	}
}