namespace Deveel.Messaging
{
    public static class TwilioSmsBuilderExtensions
    {
        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder)
            => builder.AddConnector<TwilioSmsConnector>();

        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TwilioSmsConnector>(connectionString);

        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TwilioSmsConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TwilioSmsConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddTwilioSms(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TwilioSmsConnector>(name, connectionString);
    }
}
