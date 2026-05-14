namespace Deveel.Messaging
{
    public static class TelegramMessageBuilderExtensions
    {
        public static MessageBuilder WithParseMode(this MessageBuilder builder, string parseMode)
            => builder.WithProperty("ParseMode", parseMode);

        public static MessageBuilder WithDisableWebPagePreview(this MessageBuilder builder, bool disable = true)
            => builder.WithProperty("DisableWebPagePreview", disable);

        public static MessageBuilder WithDisableNotification(this MessageBuilder builder, bool disable = true)
            => builder.WithProperty("DisableNotification", disable);

        public static MessageBuilder WithReplyToMessageId(this MessageBuilder builder, int messageId)
            => builder.WithProperty("ReplyToMessageId", messageId);

        public static MessageBuilder WithCaption(this MessageBuilder builder, string caption)
            => builder.WithProperty("Caption", caption);
    }
}
