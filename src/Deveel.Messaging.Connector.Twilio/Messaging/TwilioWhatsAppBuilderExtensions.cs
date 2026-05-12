namespace Deveel.Messaging
{
    public static class TwilioWhatsAppBuilderExtensions
    {
        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder)
            => builder.AddConnector<TwilioWhatsAppConnector>();

        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TwilioWhatsAppConnector>(connectionString);

        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TwilioWhatsAppConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TwilioWhatsAppConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddTwilioWhatsApp(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TwilioWhatsAppConnector>(name, connectionString);
    }
}
