//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// An implementation of <see cref="ConnectorTypeCatalog"/> that resolves
    /// connector type entries from the dependency injection container.
    /// </summary>
    public sealed class ServiceProviderConnectorTypeCatalog : ConnectorTypeCatalog
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<ConnectorTypeRegistration> _registrations;

        /// <summary>
        /// Constructs the catalog with a service provider and registrations.
        /// </summary>
        /// <param name="serviceProvider">The service provider for keyed resolution.</param>
        /// <param name="registrations">All registered connector type registrations.</param>
        public ServiceProviderConnectorTypeCatalog(
            IServiceProvider serviceProvider,
            IEnumerable<ConnectorTypeRegistration> registrations)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));

            foreach (var reg in registrations)
                Register(reg.Name, reg.ConnectorType);
        }
    }
}
