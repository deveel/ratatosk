namespace Deveel.Messaging
{
    public static class FirebasePushBuilderExtensions
    {
        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder)
            => builder.AddConnector<FirebasePushConnector>();

        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<FirebasePushConnector>(connectionString);

        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, Action<ChannelConnectorBuilder<FirebasePushConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<FirebasePushConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddFirebasePush(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<FirebasePushConnector>(name, connectionString);
    }
}
