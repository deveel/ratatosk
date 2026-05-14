namespace Deveel.Messaging
{
    internal static class MessagingClientLoggerEventId
    {
        private const int BaseId = 5001;

        public const int ClientResolvingChannel = BaseId + 0;
        public const int ClientChannelResolved = BaseId + 1;
        public const int ClientChannelNotFound = BaseId + 2;

        public const int ClientSendingMessage = BaseId + 10;
        public const int ClientMessageSent = BaseId + 11;
        public const int ClientMessageSendFailed = BaseId + 12;

        public const int ClientReceivingMessage = BaseId + 20;
        public const int ClientMessageReceived = BaseId + 21;
        public const int ClientMessageReceiveFailed = BaseId + 22;

        public const int ClientReadingStatus = BaseId + 30;
        public const int ClientStatusRead = BaseId + 31;
        public const int ClientStatusReadFailed = BaseId + 32;

        public const int ClientReceivingMessageStatus = BaseId + 40;
        public const int ClientMessageStatusReceived = BaseId + 41;
        public const int ClientMessageStatusReceiveFailed = BaseId + 42;

        public const int ClientInitializingConnector = BaseId + 50;
        public const int ClientConnectorInitialized = BaseId + 51;
        public const int ClientConnectorInitializationFailed = BaseId + 52;
    }
}
