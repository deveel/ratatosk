namespace Deveel.Messaging;

internal static class ConnectorLoggerEventId
{
    public const int StartingAuthentication = LoggerEventId.BaseId + 10;
    public const int NoAuthenticationConfigurationFound = LoggerEventId.BaseId + 11;
    public const int UsingAuthenticationConfiguration = LoggerEventId.BaseId + 12;
    public const int AuthenticationSuccessful = LoggerEventId.BaseId + 13;
    public const int AuthenticationFailed = LoggerEventId.BaseId + 14;
    public const int AuthenticationException = LoggerEventId.BaseId + 15;
    public const int NoCredentialToRefresh = LoggerEventId.BaseId + 16;
    public const int RefreshingAuthenticationCredential = LoggerEventId.BaseId + 17;
    public const int AuthenticationCredentialRefreshed = LoggerEventId.BaseId + 18;
    public const int AuthenticationCredentialRefreshFailed = LoggerEventId.BaseId + 19;
    public const int AuthenticationCredentialRefreshException = LoggerEventId.BaseId + 20;

    public const int CheckingHealth = LoggerEventId.BaseId + 25;
    public const int HealthCheckSuccessful = LoggerEventId.BaseId + 26;
    public const int HealthCheckFailed = LoggerEventId.BaseId + 27;

    public const int StateChanged = LoggerEventId.BaseId + 30;
    public const int InitializingConnector = LoggerEventId.BaseId + 31;
    public const int ConnectorInitialized = LoggerEventId.BaseId + 32;
    public const int ConnectorInitializationFailed = LoggerEventId.BaseId + 33;
    public const int ReadingStatus = LoggerEventId.BaseId + 34;
    public const int StatusRead = LoggerEventId.BaseId + 35;
    public const int StatusReadFailed = LoggerEventId.BaseId + 36;

    public const int TestingConnection = LoggerEventId.BaseId + 40;
    public const int ConnectionTestSuccessful = LoggerEventId.BaseId + 41;
    public const int ConnectionTestFailed = LoggerEventId.BaseId + 43;

    public const int ValidatingMessage = LoggerEventId.BaseId + 50;
    public const int MessageValidationFailed = LoggerEventId.BaseId + 51;
    public const int MessageValidationPassed = LoggerEventId.BaseId + 52;
    public const int BatchValidationFailed = LoggerEventId.BaseId + 53;

    public const int ValidatingCapability = LoggerEventId.BaseId + 60;
    public const int CapabilityNotSupported = LoggerEventId.BaseId + 61;

    public const int ValidatingOperationalState = LoggerEventId.BaseId + 71;
    public const int NotInOperationalState = LoggerEventId.BaseId + 72;

    public const int SendingMessage = LoggerEventId.BaseId + 80;
    public const int MessageSent = LoggerEventId.BaseId + 81;
    public const int MessageSendFailed = LoggerEventId.BaseId + 82;
    public const int ReadingMessageStatus = LoggerEventId.BaseId + 83;
    public const int MessageStatusRead = LoggerEventId.BaseId + 84;
    public const int MessageStatusReadFailed = LoggerEventId.BaseId + 85;
    public const int SendingBatch = LoggerEventId.BaseId + 90;
    public const int BatchSent = LoggerEventId.BaseId + 91;
    public const int BatchSendFailed = LoggerEventId.BaseId + 92;

    public const int ReceivingMessageStatus = LoggerEventId.BaseId + 100;
    public const int MessageStatusReceived = LoggerEventId.BaseId + 101;
    public const int MessageStatusReceiveFailed = LoggerEventId.BaseId + 102;

    public const int ReceivingMessage = LoggerEventId.BaseId + 110;
    public const int MessageReceived = LoggerEventId.BaseId + 111;
    public const int MessageReceiveFailed = LoggerEventId.BaseId + 112;
}
