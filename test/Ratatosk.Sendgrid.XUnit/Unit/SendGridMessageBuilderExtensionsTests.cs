namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "SendGridMessageBuilder")]
    public class SendGridMessageBuilderExtensionsTests
    {
        [Fact]
        public void Should_SetTemplateId_When_WithTemplateId()
        {
            var builder = new MessageBuilder();
            var result = builder.WithTemplateId("d-abc123");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetCategories_When_WithCategories()
        {
            var builder = new MessageBuilder();
            var result = builder.WithCategories("[\"category1\"]");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetCustomArgs_When_WithCustomArgs()
        {
            var builder = new MessageBuilder();
            var result = builder.WithCustomArgs("{\"arg1\":\"val1\"}");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetSendAt_When_WithSendAt()
        {
            var builder = new MessageBuilder();
            var result = builder.WithSendAt(DateTime.UtcNow.AddHours(1));
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetPriority_When_WithPriority()
        {
            var builder = new MessageBuilder();
            var result = builder.WithPriority("high");
            Assert.Same(builder, result);
        }
    }
}
