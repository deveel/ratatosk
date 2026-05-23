using System.Diagnostics.CodeAnalysis;

using Twilio;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging
{
    [ExcludeFromCodeCoverage]
    public class TwilioApiClient : ITwilioApiClient
    {
        public void Initialize(string accountSid, string authToken)
        {
            TwilioClient.Init(accountSid, authToken);
        }

        public Task<AccountResource?> FetchAccountAsync(string accountSid)
            => AccountResource.FetchAsync(accountSid);

        public Task<MessageResource> CreateMessageAsync(CreateMessageOptions options)
            => MessageResource.CreateAsync(options);

        public Task<MessageResource> FetchMessageAsync(string messageSid)
            => MessageResource.FetchAsync(messageSid);
    }
}
