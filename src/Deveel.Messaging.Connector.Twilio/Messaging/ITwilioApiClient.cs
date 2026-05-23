using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging
{
    public interface ITwilioApiClient
    {
        void Initialize(string accountSid, string authToken);

        Task<AccountResource?> FetchAccountAsync(string accountSid);

        Task<MessageResource> CreateMessageAsync(CreateMessageOptions options);

        Task<MessageResource> FetchMessageAsync(string messageSid);
    }
}
