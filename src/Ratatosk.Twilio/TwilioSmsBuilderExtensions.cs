namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to register
    /// the Twilio SMS connector.
    /// </summary>
    public static class TwilioSmsBuilderExtensions
    {
        /// <summary>
        /// Adds the Twilio SMS connector to the messaging builder.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder)
            => builder.AddConnector<TwilioSmsConnector>();

        /// <summary>
        /// Adds the Twilio SMS connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TwilioSmsConnector>(connectionString);

        /// <summary>
        /// Adds the Twilio SMS connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TwilioSmsConnector>> configure)
            => builder.AddConnector(configure);

        /// <summary>
        /// Adds a named Twilio SMS connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TwilioSmsConnector>> configure)
            => builder.AddConnector(name, configure);

        /// <summary>
        /// Adds a named Twilio SMS connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TwilioSmsConnector>(name, connectionString);
    }
}
