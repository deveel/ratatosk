namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to configure
    /// Telegram Bot-specific message properties.
    /// </summary>
    public static class TelegramMessageBuilderExtensions
    {
        /// <summary>
        /// Sets the parse mode for the message text.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="parseMode">The parse mode (e.g., Markdown, HTML).</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithParseMode(this MessageBuilder builder, string parseMode)
            => builder.WithProperty("ParseMode", parseMode);

        /// <summary>
        /// Disables the web page preview for links in the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="disable"><c>true</c> to disable the preview; otherwise <c>false</c>.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithDisableWebPagePreview(this MessageBuilder builder, bool disable = true)
            => builder.WithProperty("DisableWebPagePreview", disable);

        /// <summary>
        /// Disables notification for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="disable"><c>true</c> to disable notification; otherwise <c>false</c>.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithDisableNotification(this MessageBuilder builder, bool disable = true)
            => builder.WithProperty("DisableNotification", disable);

        /// <summary>
        /// Sets the message identifier this message is replying to.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="messageId">The identifier of the original message.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithReplyToMessageId(this MessageBuilder builder, int messageId)
            => builder.WithProperty("ReplyToMessageId", messageId);

        /// <summary>
        /// Sets the caption for media messages.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="caption">The caption text.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithCaption(this MessageBuilder builder, string caption)
            => builder.WithProperty("Caption", caption);
    }
}
