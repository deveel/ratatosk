namespace Deveel.Messaging
{
    public static class FirebaseMessageBuilderExtensions
    {
        public static MessageBuilder WithTitle(this MessageBuilder builder, string title)
            => builder.WithProperty("Title", title);

        public static MessageBuilder WithImageUrl(this MessageBuilder builder, string imageUrl)
            => builder.WithProperty("ImageUrl", imageUrl);

        public static MessageBuilder WithCustomData(this MessageBuilder builder, string customDataJson)
            => builder.WithProperty("CustomData", customDataJson);

        public static MessageBuilder WithPriority(this MessageBuilder builder, string priority)
            => builder.WithProperty("Priority", priority);

        public static MessageBuilder WithTimeToLive(this MessageBuilder builder, int seconds)
            => builder.WithProperty("TimeToLive", seconds);

        public static MessageBuilder WithBadge(this MessageBuilder builder, int badge)
            => builder.WithProperty("Badge", badge);

        public static MessageBuilder WithSound(this MessageBuilder builder, string sound)
            => builder.WithProperty("Sound", sound);
    }
}
