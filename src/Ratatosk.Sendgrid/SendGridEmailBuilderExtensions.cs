namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to register
    /// the SendGrid email connector.
    /// </summary>
    public static class SendGridEmailBuilderExtensions
    {
        /// <summary>
        /// Adds the SendGrid email connector to the messaging builder.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder)
            => builder.AddConnector<SendGridEmailConnector>();

        /// <summary>
        /// Adds the SendGrid email connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<SendGridEmailConnector>(connectionString);

        /// <summary>
        /// Adds the SendGrid email connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, Action<ChannelConnectorBuilder<SendGridEmailConnector>> configure)
            => builder.AddConnector(configure);

        /// <summary>
        /// Adds a named SendGrid email connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<SendGridEmailConnector>> configure)
            => builder.AddConnector(name, configure);

        /// <summary>
        /// Adds a named SendGrid email connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<SendGridEmailConnector>(name, connectionString);
    }
}
