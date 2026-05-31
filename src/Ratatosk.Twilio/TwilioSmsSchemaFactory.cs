namespace Ratatosk
{
	class TwilioSmsSchemaFactory : IChannelSchemaFactory
	{
		public IChannelSchema CreateSchema() => TwilioChannelSchemas.TwilioSms;
	}
}
