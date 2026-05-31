namespace Ratatosk.Senders
{
    /// <summary>
    /// The Entity Framework entity used to persist sender identities
    /// in a relational database.
    /// </summary>
    public class DbSender : ISender
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sender.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical name of the sender.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint address (e.g. email address, phone number).
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint type stored as a string.
        /// </summary>
        public string Type { get; set; } = EndpointType.Any.ToString();

        /// <summary>
        /// Gets or sets a value indicating whether the sender is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the timestamp when the sender was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the sender was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EndpointType"/> parsed from <see cref="Type"/>.
        /// </summary>
        public EndpointType EndpointType
        {
            get => Enum.TryParse<EndpointType>(Type, out var type) ? type : EndpointType.Any;
            set => Type = value.ToString();
        }

        /// <inheritdoc />
        EndpointType IEndpoint.Type => EndpointType;

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
    }
}
