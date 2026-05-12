namespace Deveel.Messaging
{
    class SendGridEmailSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => SendGridChannelSchemas.SendGridEmail;
    }
}
