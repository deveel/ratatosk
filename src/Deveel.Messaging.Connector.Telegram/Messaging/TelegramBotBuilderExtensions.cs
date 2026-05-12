namespace Deveel.Messaging
{
    public static class TelegramBotBuilderExtensions
    {
        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder)
            => builder.AddConnector<TelegramBotConnector>();

        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string connectionString)
            => builder.AddConnector<TelegramBotConnector>(connectionString);

        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, Action<ChannelConnectorBuilder<TelegramBotConnector>> configure)
            => builder.AddConnector(configure);

        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string name, Action<ChannelConnectorBuilder<TelegramBotConnector>> configure)
            => builder.AddConnector(name, configure);

        public static MessagingBuilder AddTelegramBot(this MessagingBuilder builder, string name, string connectionString)
            => builder.AddConnector<TelegramBotConnector>(name, connectionString);
    }
}
