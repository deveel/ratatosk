namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to configure
    /// Firebase Cloud Messaging-specific message properties.
    /// </summary>
    public static class FirebaseMessageBuilderExtensions
    {
        /// <summary>
        /// Sets the title of the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="title">The title text.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithTitle(this MessageBuilder builder, string title)
            => builder.WithProperty("Title", title);

        /// <summary>
        /// Sets the image URL for the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="imageUrl">The URL of the image.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithImageUrl(this MessageBuilder builder, string imageUrl)
            => builder.WithProperty("ImageUrl", imageUrl);

        /// <summary>
        /// Sets custom data payload for the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="customDataJson">A JSON string containing the custom data.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithCustomData(this MessageBuilder builder, string customDataJson)
            => builder.WithProperty("CustomData", customDataJson);

        /// <summary>
        /// Sets the priority of the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="priority">The priority level (e.g., normal, high).</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithPriority(this MessageBuilder builder, string priority)
            => builder.WithProperty("Priority", priority);

        /// <summary>
        /// Sets the time-to-live of the push notification in seconds.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="seconds">The time-to-live in seconds.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithTimeToLive(this MessageBuilder builder, int seconds)
            => builder.WithProperty("TimeToLive", seconds);

        /// <summary>
        /// Sets the badge number for the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="badge">The badge number.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithBadge(this MessageBuilder builder, int badge)
            => builder.WithProperty("Badge", badge);

        /// <summary>
        /// Sets the sound for the push notification.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="sound">The sound file name or default.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithSound(this MessageBuilder builder, string sound)
            => builder.WithProperty("Sound", sound);
    }
}
