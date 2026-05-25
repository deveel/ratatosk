namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "FirebaseMessageBuilder")]
    public class FirebaseMessageBuilderExtensionsTests
    {
        [Fact]
        public void Should_SetTitle_When_WithTitle()
        {
            var builder = new MessageBuilder();
            var result = builder.WithTitle("Test Title");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetImageUrl_When_WithImageUrl()
        {
            var builder = new MessageBuilder();
            var result = builder.WithImageUrl("https://example.com/image.png");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetCustomData_When_WithCustomData()
        {
            var builder = new MessageBuilder();
            var result = builder.WithCustomData("{\"key\":\"value\"}");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetPriority_When_WithPriority()
        {
            var builder = new MessageBuilder();
            var result = builder.WithPriority("high");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetTimeToLive_When_WithTimeToLive()
        {
            var builder = new MessageBuilder();
            var result = builder.WithTimeToLive(3600);
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetBadge_When_WithBadge()
        {
            var builder = new MessageBuilder();
            var result = builder.WithBadge(1);
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetSound_When_WithSound()
        {
            var builder = new MessageBuilder();
            var result = builder.WithSound("default");
            Assert.Same(builder, result);
        }
    }
}
