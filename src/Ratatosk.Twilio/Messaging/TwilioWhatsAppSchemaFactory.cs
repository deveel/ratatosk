namespace Ratatosk
{
	class TwilioWhatsAppSchemaFactory : IChannelSchemaFactory
	{
		public IChannelSchema CreateSchema() => TwilioChannelSchemas.TwilioWhatsApp;
	}
}
