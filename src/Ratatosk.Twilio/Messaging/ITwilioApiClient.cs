using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Ratatosk
{
    /// <summary>
    /// Provides an abstraction over the Twilio SDK for creating and fetching messages and accounts.
    /// </summary>
    public interface ITwilioApiClient
    {
        /// <summary>
        /// Initializes the Twilio client with the given account credentials.
        /// </summary>
        /// <param name="accountSid">The Twilio Account SID.</param>
        /// <param name="authToken">The Twilio authentication token.</param>
        void Initialize(string accountSid, string authToken);

        /// <summary>
        /// Fetches the Twilio account information for the given account SID.
        /// </summary>
        /// <param name="accountSid">The Twilio Account SID to fetch.</param>
        /// <returns>
        /// A <see cref="AccountResource"/> representing the account, or <c>null</c> if not found.
        /// </returns>
        Task<AccountResource?> FetchAccountAsync(string accountSid);

        /// <summary>
        /// Creates a new message using the given options.
        /// </summary>
        /// <param name="options">The options describing the message to create.</param>
        /// <returns>A <see cref="MessageResource"/> representing the created message.</returns>
        Task<MessageResource> CreateMessageAsync(CreateMessageOptions options);

        /// <summary>
        /// Fetches a message by its SID from the Twilio API.
        /// </summary>
        /// <param name="messageSid">The SID of the message to fetch.</param>
        /// <returns>A <see cref="MessageResource"/> representing the fetched message.</returns>
        Task<MessageResource> FetchMessageAsync(string messageSid);
    }
}
