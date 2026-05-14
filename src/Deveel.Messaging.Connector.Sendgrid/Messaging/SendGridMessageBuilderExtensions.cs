namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to configure
    /// SendGrid-specific message properties.
    /// </summary>
    public static class SendGridMessageBuilderExtensions
    {
        /// <summary>
        /// Sets the template identifier for the email.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="templateId">The SendGrid template identifier.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithTemplateId(this MessageBuilder builder, string templateId)
            => builder.WithProperty("TemplateId", templateId);

        /// <summary>
        /// Sets the categories for the email.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="categories">A JSON string containing the category list.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithCategories(this MessageBuilder builder, string categories)
            => builder.WithProperty("Categories", categories);

        /// <summary>
        /// Sets custom arguments for the email.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="customArgsJson">A JSON string containing custom arguments.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithCustomArgs(this MessageBuilder builder, string customArgsJson)
            => builder.WithProperty("CustomArgs", customArgsJson);

        /// <summary>
        /// Sets the scheduled send time for the email.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="sendAt">The time at which to send the email.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithSendAt(this MessageBuilder builder, DateTime sendAt)
            => builder.WithProperty("SendAt", sendAt);

        /// <summary>
        /// Sets the priority for the email.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="priority">The priority level.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithPriority(this MessageBuilder builder, string priority)
            => builder.WithProperty("Priority", priority);
    }
}
