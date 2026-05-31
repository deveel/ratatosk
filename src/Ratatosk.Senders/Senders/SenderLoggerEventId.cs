namespace Ratatosk.Senders;

static class SenderLoggerEventId
{
    private const int BaseId = 7001;

    public const int SenderResolvedFromCache = BaseId + 0;
    public const int SenderNotFoundInRegistry = BaseId + 1;
    public const int NoSenderFoundForEndpoint = BaseId + 2;
    public const int SenderFoundButInactive = BaseId + 3;
    public const int FailedToFindSenderByName = BaseId + 4;
    public const int FailedToFindSenderByEndpoint = BaseId + 5;
    public const int SenderResolvedFromCacheByEndpoint = BaseId + 6;
    public const int FailedToRetrieveAllActiveSenders = BaseId + 7;
    public const int FailedToSetActiveState = BaseId + 8;
}
