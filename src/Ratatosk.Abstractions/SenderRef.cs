//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// A lightweight reference to a stored sender identity, used in message
    /// building to defer sender resolution until send time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a message is built with <c>FromSender(name)</c>, the sender is stored
    /// as a <see cref="SenderRef"/> rather than a fully resolved sender. At send
    /// time, the sender resolver dereferences this to the concrete
    /// <see cref="ISender"/> registered in the sender store.
    /// </para>
    /// <para>
    /// The <see cref="EndpointType"/> is set to <see cref="EndpointType.Any"/>
    /// so that pre-resolution schema validation does not reject the reference
    /// due to an unknown endpoint type.
    /// </para>
    /// </remarks>
    public class SenderRef : ISender
    {
        /// <summary>
        /// Constructs a new sender reference with the given name.
        /// </summary>
        /// <param name="senderName">
        /// The logical name of the sender to resolve at send time.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="senderName"/> is null or whitespace.
        /// </exception>
        [System.Text.Json.Serialization.JsonConstructor]
        public SenderRef(string senderName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(senderName, nameof(senderName));
            SenderName = senderName;
        }

        /// <summary>
        /// Constructs a sender reference from an existing sender identity,
        /// using its <see cref="ISender.Name"/> as the reference name.
        /// </summary>
        /// <param name="sender">
        /// The sender identity to create a reference for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sender"/> is <c>null</c>.
        /// </exception>
        public SenderRef(ISender sender)
            : this(sender.Name)
        {
        }

        /// <summary>
        /// Gets the logical name of the sender to resolve.
        /// </summary>
        public string SenderName { get; }

        /// <inheritdoc />
        public string Name => SenderName;

        /// <inheritdoc />
        public string DisplayName => SenderName;

        /// <inheritdoc />
        public bool IsActive => true;

        /// <inheritdoc />
        public EndpointType Type => EndpointType.Any;

        /// <inheritdoc />
        public string Address => SenderName;
    }
}
