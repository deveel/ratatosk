namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to register
    /// the Telegram Bot connector.
    /// </summary>
    public static class TelegramBotBuilderExtensions
    {
        /// <summary>
        /// Adds the Telegram Bot connector to the messaging builder.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder)
            => builder.AddConnector<TelegramBotConnector>();

        /// <summary>
        /// Adds the Telegram Bot connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TelegramBotConnector>(connectionString);

        /// <summary>
        /// Adds the Telegram Bot connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TelegramBotConnector>> configure)
            => builder.AddConnector(configure);

        /// <summary>
        /// Adds a named Telegram Bot connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TelegramBotConnector>> configure)
            => builder.AddConnector(name, configure);

        /// <summary>
        /// Adds a named Telegram Bot connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TelegramBotConnector>(name, connectionString);
    }
}
