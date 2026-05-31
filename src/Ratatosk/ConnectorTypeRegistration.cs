//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a registered connector type entry for enumeration
    /// by the <see cref="ConnectorTypeCatalog"/>.
    /// </summary>
    public sealed class ConnectorTypeRegistration
    {
        /// <summary>
        /// Gets the name under which the connector type is registered.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the connector implementation.
        /// </summary>
        public Type ConnectorType { get; }

        /// <summary>
        /// Constructs a new registration entry.
        /// </summary>
        /// <param name="name">The registered name.</param>
        /// <param name="connectorType">The connector type.</param>
        public ConnectorTypeRegistration(string name, Type connectorType)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(connectorType, nameof(connectorType));

            Name = name;
            ConnectorType = connectorType;
        }
    }
}
