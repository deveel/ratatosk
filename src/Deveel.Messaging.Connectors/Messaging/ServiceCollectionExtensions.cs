//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
	/// <summary>
	/// Provides extension methods for <see cref="IServiceCollection"/> to configure
	/// messaging services.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the messaging services to the service collection and returns a
		/// <see cref="MessagingBuilder"/> for further configuration.
		/// </summary>
		/// <param name="services">The service collection to configure.</param>
		/// <returns>A <see cref="MessagingBuilder"/> for fluent configuration.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
		/// <example>
		/// <code>
		/// services.AddMessaging()
		///     .AddConnector&lt;TwilioSmsConnector&gt;()
		///     .AddConnector&lt;TwilioSmsConnector&gt;("marketing");
		/// </code>
		/// </example>
		public static MessagingBuilder AddMessaging(this IServiceCollection services)
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));

			return new MessagingBuilder(services);
		}

		/// <summary>
		/// Registers a single channel connector type with automatic schema discovery.
		/// </summary>
		/// <typeparam name="TConnector">The type of connector to register.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="connectorFactory">An optional factory function to create connector instances.</param>
		/// <returns>The service collection for method chaining.</returns>
		[Obsolete("Use services.AddMessaging().AddConnector<TConnector>() instead.")]
		public static IServiceCollection AddChannelConnector<TConnector>(
			this IServiceCollection services,
			Func<IServiceProvider, IChannelSchema, TConnector>? connectorFactory = null)
			where TConnector : class, IChannelConnector
		{
			services.AddMessaging().AddConnector(connectorFactory);
			return services;
		}

		/// <summary>
		/// Registers a single channel connector type with automatic schema discovery.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="connectorType">The type of connector to register.</param>
		/// <param name="connectorFactory">An optional factory function to create connector instances.</param>
		/// <returns>The service collection for method chaining.</returns>
		[Obsolete("Use services.AddMessaging().AddConnector(connectorType) instead.")]
		public static IServiceCollection AddChannelConnector(
			this IServiceCollection services,
			Type connectorType,
			Func<IServiceProvider, IChannelSchema, IChannelConnector>? connectorFactory = null)
		{
			services.AddMessaging().AddConnector(connectorType, connectorFactory);

			return services;
		}
	}
}