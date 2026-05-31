using System.ComponentModel.DataAnnotations;

namespace Ratatosk.Senders
{
    /// <summary>
    /// The default entity used to persist sender identities in storage.
    /// Implements <see cref="ISender"/> directly and is used by
    /// the in-memory sender store.
    /// </summary>
    public class SenderEntity : ISender
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sender entity.
        /// </summary>
        [Key]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical name used to reference the sender.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable display name of the sender.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint address of the sender
        /// (e.g. email address, phone number).
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint type tag that indicates
        /// what kind of address <see cref="Address"/> holds.
        /// </summary>
        public EndpointType Type { get; set; } = EndpointType.Any;

        /// <summary>
        /// Gets or sets a value indicating whether the sender
        /// is active and can be used to send messages.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Gets or sets the timestamp when the sender was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the sender was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <inheritdoc />
        EndpointType IEndpoint.Type => Type;

        /// <summary>
        /// Activates the sender.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivates the sender.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the sender properties with the provided values.
        /// </summary>
        /// <param name="displayName">The new display name, or null to keep the current value.</param>
        /// <param name="address">The new address, or null to keep the current value.</param>
        /// <param name="type">The new endpoint type, or null to keep the current value.</param>
        public void Update(string? displayName = null, string? address = null, EndpointType? type = null)
        {
            if (displayName is not null)
                DisplayName = displayName;
            
            if (address is not null)
                Address = address;
            
            if (type.HasValue)
                Type = type.Value;
            
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
