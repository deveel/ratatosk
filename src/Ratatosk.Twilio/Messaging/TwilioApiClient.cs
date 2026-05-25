using System.Diagnostics.CodeAnalysis;

using Twilio;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Ratatosk
{
    [ExcludeFromCodeCoverage]
    public class TwilioApiClient : ITwilioApiClient
    {
        /// <inheritdoc/>
        public void Initialize(string accountSid, string authToken)
        {
            TwilioClient.Init(accountSid, authToken);
        }

        /// <inheritdoc/>
        public Task<AccountResource?> FetchAccountAsync(string accountSid)
            => AccountResource.FetchAsync(accountSid);

        /// <inheritdoc/>
        public Task<MessageResource> CreateMessageAsync(CreateMessageOptions options)
            => MessageResource.CreateAsync(options);

        /// <inheritdoc/>
        public Task<MessageResource> FetchMessageAsync(string messageSid)
            => MessageResource.FetchAsync(messageSid);
    }
}
