using System.Collections.Generic;

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
		private string? _logFragment;

		private IList<IInterceptor>? _interceptors;

		/// <summary>
		/// List of registered LinqToDB interceptors
		/// </summary>
		public virtual IList<IInterceptor> Interceptors => _interceptors ??= new List<IInterceptor>();

		/// <inheritdoc cref="IDbContextOptionsExtension.LogFragment"/>
		public string LogFragment
		{
			get
			{
				if (_logFragment == null)
				{
					string logFragment = string.Empty;

					if (_interceptors?.Count > 0)
					{
						_logFragment = $"Interceptors count: {_interceptors.Count}";
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
		}

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="copyFrom"></param>
		protected LinqToDBOptionsExtension(LinqToDBOptionsExtension copyFrom)
		{
			_interceptors = copyFrom._interceptors;
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
