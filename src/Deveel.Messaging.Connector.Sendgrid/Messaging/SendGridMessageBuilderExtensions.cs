namespace Deveel.Messaging
{
    public static class SendGridMessageBuilderExtensions
    {
        public static MessageBuilder WithTemplateId(this MessageBuilder builder, string templateId)
            => builder.WithProperty("TemplateId", templateId);

        public static MessageBuilder WithCategories(this MessageBuilder builder, string categories)
            => builder.WithProperty("Categories", categories);

        public static MessageBuilder WithCustomArgs(this MessageBuilder builder, string customArgsJson)
            => builder.WithProperty("CustomArgs", customArgsJson);

        public static MessageBuilder WithSendAt(this MessageBuilder builder, DateTime sendAt)
            => builder.WithProperty("SendAt", sendAt);

        public static MessageBuilder WithPriority(this MessageBuilder builder, string priority)
            => builder.WithProperty("Priority", priority);
    }
}
