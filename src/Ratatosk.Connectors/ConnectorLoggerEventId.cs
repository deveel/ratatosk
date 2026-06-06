namespace Ratatosk;

internal static class ConnectorLoggerEventId
{
    public const int StartingAuthentication = LoggerEventId.BaseId + 10;
    public const int NoAuthenticationConfigurationFound = LoggerEventId.BaseId + 11;
    public const int NoAuthenticationConfigurationFoundForType = LoggerEventId.BaseId + 12;
    public const int UsingAuthenticationConfiguration = LoggerEventId.BaseId + 13;
    public const int AuthenticationSuccessful = LoggerEventId.BaseId + 14;
    public const int AuthenticationFailed = LoggerEventId.BaseId + 15;
    public const int AuthenticationException = LoggerEventId.BaseId + 16;
    public const int NoCredentialToRefresh = LoggerEventId.BaseId + 17;
    public const int RefreshingAuthenticationCredential = LoggerEventId.BaseId + 18;
    public const int AuthenticationCredentialRefreshed = LoggerEventId.BaseId + 19;
    public const int AuthenticationCredentialRefreshFailed = LoggerEventId.BaseId + 20;
    public const int AuthenticationCredentialRefreshException = LoggerEventId.BaseId + 21;

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

    // Authentication Manager events
    public const int AuthenticationProviderRegistered = LoggerEventId.BaseId + 120;
    public const int AuthenticationProviderNotFound = LoggerEventId.BaseId + 121;
    public const int UsingCachedCredential = LoggerEventId.BaseId + 122;
    public const int ObtainingNewCredential = LoggerEventId.BaseId + 123;
    public const int AuthenticationFailedWithMessage = LoggerEventId.BaseId + 124;
    public const int CacheCleared = LoggerEventId.BaseId + 125;
    public const int CredentialInvalidated = LoggerEventId.BaseId + 126;
    public const int DefaultProvidersRegistered = LoggerEventId.BaseId + 127;
    public const int AutoAuthenticationFailed = LoggerEventId.BaseId + 128;

    // Provider-specific events
    public const int FoundCredentials = LoggerEventId.BaseId + 130;
    public const int ObtainingAccessToken = LoggerEventId.BaseId + 131;
    public const int TokenRequestSent = LoggerEventId.BaseId + 132;
    public const int TokenRequestFailed = LoggerEventId.BaseId + 133;
    public const int MissingAccessToken = LoggerEventId.BaseId + 134;
    public const int TokenObtained = LoggerEventId.BaseId + 135;
    public const int NetworkErrorDuringTokenRequest = LoggerEventId.BaseId + 136;
    public const int TokenRequestTimedOut = LoggerEventId.BaseId + 137;
    public const int TokenParseFailed = LoggerEventId.BaseId + 138;
    public const int UnexpectedTokenError = LoggerEventId.BaseId + 139;
    public const int RefreshingWithRefreshToken = LoggerEventId.BaseId + 140;
    public const int NoRefreshTokenAvailable = LoggerEventId.BaseId + 141;
    public const int TokenRefreshError = LoggerEventId.BaseId + 142;
    public const int TokenRefreshFailedWithStatus = LoggerEventId.BaseId + 143;
    public const int RetryingTokenObtainment = LoggerEventId.BaseId + 144;
    public const int TokenRefreshed = LoggerEventId.BaseId + 145;

    // Retry Policy events
    public const int RetryAttempt = LoggerEventId.BaseId + 150;
    public const int RetrySucceeded = LoggerEventId.BaseId + 151;
    public const int RetryExhausted = LoggerEventId.BaseId + 152;
    public const int CircuitBreakerOpened = LoggerEventId.BaseId + 153;
    public const int CircuitBreakerReset = LoggerEventId.BaseId + 154;
}
