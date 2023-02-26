﻿using System.Linq;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	/// <summary>
	/// Model containing LinqToDB related context options.
	/// </summary>
	public class LinqToDBOptionsExtension : IDbContextOptionsExtension
	{
		private string? _logFragment;

		/// <summary>
		/// List of registered LinqToDB interceptors
		/// </summary>
		public virtual DataOptions Options { get; set; }

		/// <inheritdoc cref="IDbContextOptionsExtension.LogFragment"/>
		public string LogFragment
		{
			get
			{
				if (_logFragment == null)
				{
					string logFragment = string.Empty;

					if (Options.DataContextOptions.Interceptors?.Count > 0)
					{
						_logFragment = $"Interceptors count: {Options.DataContextOptions.Interceptors.Count}";
					}
					else
					{
						_logFragment = string.Empty;
					}
				}

				return _logFragment;
			}
		}

		/// <summary>
		/// .ctor
		/// </summary>
		public LinqToDBOptionsExtension()
		{
			Options = new();
		}

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="copyFrom"></param>
		protected LinqToDBOptionsExtension(LinqToDBOptionsExtension copyFrom)
		{
			Options = copyFrom.Options;
		}

		/// <inheritdoc cref="IDbContextOptionsExtension.ApplyServices(IServiceCollection)"/>
		public bool ApplyServices(IServiceCollection services) => false;

		/// <summary>
		/// Gives the extension a chance to validate that all options in the extension are
		/// valid. Most extensions do not have invalid combinations and so this will be a
		/// no-op. If options are invalid, then an exception should be thrown.
		/// </summary>
		/// <param name="options"></param>
		public void Validate(IDbContextOptions options)
		{
						}

		/// <inheritdoc cref="IDbContextOptionsExtension.GetServiceProviderHashCode()"/>
		public long GetServiceProviderHashCode() => 0;
	}
}
