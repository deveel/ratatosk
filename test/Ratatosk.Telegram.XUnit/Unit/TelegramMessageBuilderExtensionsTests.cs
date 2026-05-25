namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TelegramMessageBuilder")]
    public class TelegramMessageBuilderExtensionsTests
    {
        [Fact]
        public void Should_SetParseMode_When_WithParseMode()
        {
            var builder = new MessageBuilder();
            var result = builder.WithParseMode("HTML");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_DisableWebPagePreview_When_WithDisableWebPagePreview()
        {
            var builder = new MessageBuilder();
            var result = builder.WithDisableWebPagePreview();
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_DisableNotification_When_WithDisableNotification()
        {
            var builder = new MessageBuilder();
            var result = builder.WithDisableNotification();
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetReplyToMessageId_When_WithReplyToMessageId()
        {
            var builder = new MessageBuilder();
            var result = builder.WithReplyToMessageId(42);
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetCaption_When_WithCaption()
        {
            var builder = new MessageBuilder();
            var result = builder.WithCaption("test caption");
            Assert.Same(builder, result);
        }
    }
}
