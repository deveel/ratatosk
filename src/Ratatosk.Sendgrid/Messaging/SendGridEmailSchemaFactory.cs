namespace Ratatosk
{
    class SendGridEmailSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => SendGridChannelSchemas.SendGridEmail;
    }
}
