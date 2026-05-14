namespace Deveel.Messaging
{
    public static class FacebookMessageBuilderExtensions
    {
        public static MessageBuilder WithMessagingType(this MessageBuilder builder, string messagingType)
            => builder.WithProperty("MessagingType", messagingType);

        public static MessageBuilder WithNotificationType(this MessageBuilder builder, string notificationType)
            => builder.WithProperty("NotificationType", notificationType);

        public static MessageBuilder WithTag(this MessageBuilder builder, string tag)
            => builder.WithProperty("Tag", tag);

        public static MessageBuilder WithQuickReplies(this MessageBuilder builder, string quickRepliesJson)
            => builder.WithProperty("QuickReplies", quickRepliesJson);
    }
}
