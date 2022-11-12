﻿using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	using Interceptors;

	/// <summary>
	/// Model containing LinqToDB related context options
	/// </summary>
	public class LinqToDBOptionsExtension : IDbContextOptionsExtension
	{
		private DbContextOptionsExtensionInfo? _info;
		private IList<IInterceptor>? _interceptors;

		/// <summary>
		/// Context options extension info object
		/// </summary>
		public DbContextOptionsExtensionInfo Info 
			=> _info ??= new LinqToDBExtensionInfo(this);

		/// <summary>
		/// List of registered LinqToDB interceptors
		/// </summary>
		public virtual IList<IInterceptor> Interceptors
			=> _interceptors ??= new List<IInterceptor>();

		/// <summary>
		/// .ctor
		/// </summary>
		public LinqToDBOptionsExtension()
		{
		}

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="copyFrom"></param>
		protected LinqToDBOptionsExtension(LinqToDBOptionsExtension copyFrom)
		{
			_interceptors = copyFrom._interceptors;
		}

		/// Adds the services required to make the selected options work. This is used when
		/// there is no external System.IServiceProvider and EF is maintaining its own service
		/// provider internally. This allows database providers (and other extensions) to
		/// register their required services when EF is creating an service provider.
		/// <param name="services">The collection to add services to</param>
		public void ApplyServices(IServiceCollection services)
		{
			;
		}

		/// <summary>
		/// Gives the extension a chance to validate that all options in the extension are
		/// valid. Most extensions do not have invalid combinations and so this will be a
		/// no-op. If options are invalid, then an exception should be thrown.
		/// </summary>
		/// <param name="options"></param>
		public void Validate(IDbContextOptions options)
		{
			;
		}

		private sealed class LinqToDBExtensionInfo : DbContextOptionsExtensionInfo
		{
			private string? _logFragment;

			public LinqToDBExtensionInfo(IDbContextOptionsExtension extension)
				: base(extension)
			{
			}

			private new LinqToDBOptionsExtension Extension
				=> (LinqToDBOptionsExtension)base.Extension;

			public override bool IsDatabaseProvider
				=> false;

			public override string LogFragment
			{
				get
				{
					if (_logFragment == null)
					{
						string logFragment = string.Empty;

						if (Extension.Interceptors.Any())
						{
							logFragment += $"Interceptors count: {Extension.Interceptors.Count}";
						}

						_logFragment = logFragment;
					}

					return _logFragment;
				}
			}

			public override int GetServiceProviderHashCode() => 0;

			public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
				=> debugInfo["Linq2Db"] = "1";

			public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
		}
	}
}
