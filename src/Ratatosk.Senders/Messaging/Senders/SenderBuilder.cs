//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Builds a <see cref="SenderEntity"/> instance using a fluent API.
    /// </summary>
    public class SenderBuilder
    {
        private string? _name;
        private string? _displayName;
        private string? _address;
        private string? _endpointType;
        private bool _isActive = true;

        /// <summary>
        /// Sets the logical name of the sender.
        /// </summary>
        /// <param name="name">The logical name.</param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the human-readable display name of the sender.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        /// <summary>
        /// Sets the endpoint address of the sender.
        /// </summary>
        /// <param name="address">The endpoint address.</param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder WithAddress(string address)
        {
            _address = address;
            return this;
        }

        /// <summary>
        /// Sets the endpoint type of the sender from an <see cref="EndpointType"/> enum.
        /// </summary>
        /// <param name="type">The endpoint type.</param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder WithEndpointType(EndpointType type)
        {
            _endpointType = type.ToString();
            return this;
        }

        /// <summary>
        /// Sets the endpoint type of the sender from a string.
        /// </summary>
        /// <param name="endpointType">The endpoint type string.</param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder WithEndpointType(string endpointType)
        {
            _endpointType = endpointType;
            return this;
        }

        /// <summary>
        /// Sets whether the sender is active.
        /// </summary>
        /// <param name="isActive">
        /// <c>true</c> if the sender is active; otherwise <c>false</c>.
        /// </param>
        /// <returns>This builder instance.</returns>
        public SenderBuilder AsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        /// <summary>
        /// Builds the <see cref="SenderEntity"/> with the configured values.
        /// </summary>
        /// <returns>
        /// A new <see cref="SenderEntity"/> instance.
        /// </returns>
        public SenderEntity Build()
        {
            return new SenderEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = _name ?? throw new InvalidOperationException("The sender name is required."),
                DisplayName = _displayName ?? _name ?? "Unknown",
                Address = _address ?? throw new InvalidOperationException("The address is required."),
                EndpointType = _endpointType ?? throw new InvalidOperationException("The endpoint type is required."),
                IsActive = _isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
