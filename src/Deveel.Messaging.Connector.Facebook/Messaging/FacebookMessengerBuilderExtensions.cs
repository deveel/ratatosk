namespace Deveel.Messaging
{
    public static class FacebookMessengerBuilderExtensions
    {
        public static MessagingBuilder AddFacebookMessenger(this MessagingBuilder builder)
            => builder.AddConnector<FacebookMessengerConnector>();

        public static MessagingBuilder AddFacebookMessenger(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<FacebookMessengerConnector>(connectionString);

        public static MessagingBuilder AddFacebookMessenger(this MessagingBuilder builder, Action<ChannelConnectorBuilder<FacebookMessengerConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddFacebookMessenger(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<FacebookMessengerConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddFacebookMessenger(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<FacebookMessengerConnector>(name, connectionString);
    }
}
