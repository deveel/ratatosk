namespace Ratatosk;

/// <summary>
/// Defines Telegram-specific event identifiers used for structured logging.
/// </summary>
public static class TelegramLoggerEventId
{
    private const int BaseId = LoggerEventId.BaseId + 3000;

    /// <summary>
    /// Event identifier used when the Telegram bot client has been initialized.
    /// </summary>
    public const int BotInitialized = BaseId + 1002;

    /// <summary>
    /// Event identifier used when Telegram connection testing succeeds.
    /// </summary>
    public const int BotConnectionTestSuccessful = BaseId + 2001;
}
