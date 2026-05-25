namespace Ratatosk
{
	class FacebookMessengerSchemaFactory : IChannelSchemaFactory
	{
		public IChannelSchema CreateSchema() => FacebookChannelSchemas.FacebookMessenger;
	}
}