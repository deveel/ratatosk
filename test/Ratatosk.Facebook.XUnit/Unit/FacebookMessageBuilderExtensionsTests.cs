namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "FacebookMessageBuilder")]
    public class FacebookMessageBuilderExtensionsTests
    {
        [Fact]
        public void Should_SetMessagingType_When_WithMessagingType()
        {
            var builder = new MessageBuilder();
            var result = builder.WithMessagingType("RESPONSE");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetNotificationType_When_WithNotificationType()
        {
            var builder = new MessageBuilder();
            var result = builder.WithNotificationType("REGULAR");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetTag_When_WithTag()
        {
            var builder = new MessageBuilder();
            var result = builder.WithTag("CONFIRMED_EVENT_UPDATE");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetQuickReplies_When_WithQuickReplies()
        {
            var builder = new MessageBuilder();
            var result = builder.WithQuickReplies("[{\"title\":\"Yes\"}]");
            Assert.Same(builder, result);
        }
    }
}
