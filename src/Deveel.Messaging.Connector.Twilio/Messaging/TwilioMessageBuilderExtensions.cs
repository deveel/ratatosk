namespace Deveel.Messaging
{
    public static class TwilioMessageBuilderExtensions
    {
        public static MessageBuilder WithMessagingServiceSid(this MessageBuilder builder, string messagingServiceSid)
            => builder.WithProperty(TwilioConnectionParameters.MessagingServiceSid, messagingServiceSid);

        public static MessageBuilder WithStatusCallback(this MessageBuilder builder, Uri statusCallback)
            => builder.WithProperty(TwilioConnectionParameters.StatusCallback, statusCallback);

        public static MessageBuilder WithValidityPeriod(this MessageBuilder builder, int seconds)
            => builder.WithProperty(TwilioConnectionParameters.ValidityPeriod, seconds);

        public static MessageBuilder WithMaxPrice(this MessageBuilder builder, decimal maxPrice)
            => builder.WithProperty(TwilioConnectionParameters.MaxPrice, maxPrice);
    }
}
