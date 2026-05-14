namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to configure
    /// Twilio-specific message properties.
    /// </summary>
    public static class TwilioMessageBuilderExtensions
    {
        /// <summary>
        /// Sets the Messaging Service SID for the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="messagingServiceSid">The Messaging Service SID.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithMessagingServiceSid(this MessageBuilder builder, string messagingServiceSid)
            => builder.WithProperty(TwilioConnectionParameters.MessagingServiceSid, messagingServiceSid);

        /// <summary>
        /// Sets the status callback URL for delivery status updates.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="statusCallback">The callback URL.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithStatusCallback(this MessageBuilder builder, Uri statusCallback)
            => builder.WithProperty(TwilioConnectionParameters.StatusCallback, statusCallback);

        /// <summary>
        /// Sets the validity period of the message in seconds.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="seconds">The validity period in seconds.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithValidityPeriod(this MessageBuilder builder, int seconds)
            => builder.WithProperty(TwilioConnectionParameters.ValidityPeriod, seconds);

        /// <summary>
        /// Sets the maximum price to spend on the message.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="maxPrice">The maximum price.</param>
        /// <returns>The builder instance for chaining.</returns>
        public static MessageBuilder WithMaxPrice(this MessageBuilder builder, decimal maxPrice)
            => builder.WithProperty(TwilioConnectionParameters.MaxPrice, maxPrice);
    }
}
