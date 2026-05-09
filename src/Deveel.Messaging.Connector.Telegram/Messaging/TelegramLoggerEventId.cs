namespace Deveel.Messaging;

public static class TelegramLoggerEventId
{
    private const int BaseId = LoggerEventId.BaseId + 3000;

    public const int BotInitialized = BaseId + 1002;

    public const int BotConnectionTestSuccessfull = BaseId + 2001;
}
