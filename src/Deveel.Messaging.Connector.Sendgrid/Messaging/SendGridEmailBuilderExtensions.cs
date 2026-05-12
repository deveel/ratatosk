namespace Deveel.Messaging
{
    public static class SendGridEmailBuilderExtensions
    {
        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder)
            => builder.AddConnector<SendGridEmailConnector>();

        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<SendGridEmailConnector>(connectionString);

        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, Action<ChannelConnectorBuilder<SendGridEmailConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<SendGridEmailConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddSendGridEmail(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<SendGridEmailConnector>(name, connectionString);
    }
}
