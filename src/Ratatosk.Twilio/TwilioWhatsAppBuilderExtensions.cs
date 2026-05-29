namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to register
    /// the Twilio WhatsApp connector.
    /// </summary>
    public static class TwilioWhatsAppBuilderExtensions
    {
        /// <summary>
        /// Adds the Twilio WhatsApp connector to the messaging builder.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder)
            => builder.AddConnector<TwilioWhatsAppConnector>();

        /// <summary>
        /// Adds the Twilio WhatsApp connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TwilioWhatsAppConnector>(connectionString);

        /// <summary>
        /// Adds the Twilio WhatsApp connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TwilioWhatsAppConnector>> configure)
            => builder.AddConnector(configure);

        /// <summary>
        /// Adds a named Twilio WhatsApp connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TwilioWhatsAppConnector>> configure)
            => builder.AddConnector(name, configure);

        /// <summary>
        /// Adds a named Twilio WhatsApp connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TwilioWhatsAppConnector>(name, connectionString);
    }
}
