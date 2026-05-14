namespace Deveel.Messaging
{
    /// <summary>
    /// Configures the behavior of <see cref="MessagingClient"/>.
    /// </summary>
    /// <remarks>
    /// Instances of this class are typically created by the
    /// <see cref="MessagingClientBuilderExtensions.AddClient(MessagingBuilder, Action{MessagingClientOptions})"/>
    /// extension and registered as a singleton in the dependency injection container.
    /// </remarks>
    public class MessagingClientOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the messaging client should
        /// automatically call <see cref="IChannelConnector.InitializeAsync"/>
        /// on connectors before their first use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When set to <c>true</c> (the default), the client automatically
        /// initializes connectors when they are first resolved. This is
        /// convenient for most scenarios.
        /// </para>
        /// <para>
        /// When set to <c>false</c>, connectors are resolved but not initialized.
        /// The caller must ensure connectors are initialized before use,
        /// or handle the <see cref="ConnectorState.Uninitialized"/> state
        /// explicitly.
        /// </para>
        /// </remarks>
        public bool AutoInitialize { get; set; } = true;

        /// <summary>
        /// Gets or sets the default timeout in seconds for messaging operations.
        /// </summary>
        /// <remarks>
        /// This value is used as the default timeout when creating
        /// <see cref="CancellationToken"/> instances for operations that
        /// do not provide their own. A value of <c>0</c> means no timeout.
        /// </remarks>
        public int DefaultTimeoutSeconds { get; set; } = 30;
    }
}
