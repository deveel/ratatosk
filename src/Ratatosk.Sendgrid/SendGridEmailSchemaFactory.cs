namespace Ratatosk
{
    /// <summary>
    /// Factory for creating the SendGrid email channel schema.
    /// </summary>
    class SendGridEmailSchemaFactory : IChannelSchemaFactory
    {
        /// <summary>
        /// Creates a new instance of the SendGrid email channel schema.
        /// </summary>
        /// <returns>The SendGrid email channel schema.</returns>
        public IChannelSchema CreateSchema() => SendGridChannelSchemas.SendGridEmail;
    }
}
