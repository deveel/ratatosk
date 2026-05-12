namespace Deveel.Messaging
{
    class TelegramBotConnectorSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => TelegramChannelSchemas.TelegramBot;
    }
}
