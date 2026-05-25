namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to register
    /// the Firebase Cloud Messaging connector.
    /// </summary>
    public static class FirebasePushBuilderExtensions
    {
        /// <summary>
        /// Adds the Firebase Push connector to the messaging builder.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder)
            => builder.AddConnector<FirebasePushConnector>();

        /// <summary>
        /// Adds the Firebase Push connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<FirebasePushConnector>(connectionString);

        /// <summary>
        /// Adds the Firebase Push connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, Action<ChannelConnectorBuilder<FirebasePushConnector>> configure)
            => builder.AddConnector(configure);

        /// <summary>
        /// Adds a named Firebase Push connector with configuration.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="configure">An action to configure the connector builder.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<FirebasePushConnector>> configure)
            => builder.AddConnector(name, configure);

        /// <summary>
        /// Adds a named Firebase Push connector with a connection string.
        /// </summary>
        /// <param name="builder">The messaging builder.</param>
        /// <param name="name">The name of the connector instance.</param>
        /// <param name="connectionString">The connection string for the connector.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<FirebasePushConnector>(name, connectionString);
    }
}
