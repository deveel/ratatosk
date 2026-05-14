namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to configure
    /// Facebook Messenger-specific message properties.
    /// </summary>
    public static class FacebookMessageBuilderExtensions
    {
        /// <summary>
        /// Sets the messaging type for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="messagingType">The type of messaging (e.g., RESPONSE, UPDATE, MESSAGE_TAG).</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithMessagingType(this MessageBuilder builder, string messagingType)
            => builder.WithProperty("MessagingType", messagingType);

        /// <summary>
        /// Sets the notification type for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="notificationType">The type of notification (e.g., REGULAR, SILENT_PUSH, NO_PUSH).</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithNotificationType(this MessageBuilder builder, string notificationType)
            => builder.WithProperty("NotificationType", notificationType);

        /// <summary>
        /// Sets a tag for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="tag">The tag to apply (e.g., CONFIRMED_EVENT_UPDATE, POST_PURCHASE_UPDATE).</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithTag(this MessageBuilder builder, string tag)
            => builder.WithProperty("Tag", tag);

        /// <summary>
        /// Sets the quick replies for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="quickRepliesJson">A JSON string containing the quick replies configuration.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithQuickReplies(this MessageBuilder builder, string quickRepliesJson)
            => builder.WithProperty("QuickReplies", quickRepliesJson);
    }
}
