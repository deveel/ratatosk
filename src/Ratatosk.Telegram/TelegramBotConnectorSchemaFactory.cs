namespace Ratatosk
{
    class TelegramBotConnectorSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => TelegramChannelSchemas.TelegramBot;
    }
}
