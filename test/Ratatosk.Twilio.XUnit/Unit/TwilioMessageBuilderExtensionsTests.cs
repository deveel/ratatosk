namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TwilioMessageBuilder")]
    public class TwilioMessageBuilderExtensionsTests
    {
        [Fact]
        public void Should_SetMessagingServiceSid_When_WithMessagingServiceSid()
        {
            var builder = new MessageBuilder();
            var result = builder.WithMessagingServiceSid("MG123abc");
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetStatusCallback_When_WithStatusCallback()
        {
            var builder = new MessageBuilder();
            var result = builder.WithStatusCallback(new Uri("https://example.com/callback"));
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetValidityPeriod_When_WithValidityPeriod()
        {
            var builder = new MessageBuilder();
            var result = builder.WithValidityPeriod(3600);
            Assert.Same(builder, result);
        }

        [Fact]
        public void Should_SetMaxPrice_When_WithMaxPrice()
        {
            var builder = new MessageBuilder();
            var result = builder.WithMaxPrice(0.01m);
            Assert.Same(builder, result);
        }
    }
}
