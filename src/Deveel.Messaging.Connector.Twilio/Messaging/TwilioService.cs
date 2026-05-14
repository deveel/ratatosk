//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging;

/// <summary>
/// Default implementation of <see cref="ITwilioService"/> that wraps the actual Twilio SDK calls.
/// </summary>
public class TwilioService : ITwilioService
{
    /// <inheritdoc/>
    public void Initialize(string accountSid, string authToken)
    {
        TwilioClient.Init(accountSid, authToken);
    }

    /// <inheritdoc/>
    public async Task<AccountResource?> FetchAccountAsync(string accountSid, CancellationToken cancellationToken = default)
    {
        try
        {
            return await AccountResource.FetchAsync(accountSid);
        }
        catch (ApiException ex)
        {
            throw new ConnectorException(
                MessagingErrorCodes.ConnectionFailed,
                TwilioErrorCodes.ErrorDomain,
                $"Twilio API error fetching account: {ex.Message}",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<MessageResource> CreateMessageAsync(CreateMessageOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            return await MessageResource.CreateAsync(options);
        }
        catch (ApiException ex)
        {
            var errorCode = MapTwilioErrorCode(ex.Code);

            throw new ConnectorException(
                errorCode,
                TwilioErrorCodes.ErrorDomain,
                $"Twilio API error sending message: {ex.Message}",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<MessageResource> FetchMessageAsync(string messageSid, CancellationToken cancellationToken = default)
    {
        try
        {
            return await MessageResource.FetchAsync(messageSid);
        }
        catch (ApiException ex)
        {
            throw new ConnectorException(
                TwilioErrorCodes.StatusQueryFailed,
                TwilioErrorCodes.ErrorDomain,
                $"Twilio API error fetching message status: {ex.Message}",
                ex);
        }
    }

    private static string MapTwilioErrorCode(int? twilioCode)
    {
        // Map common Twilio error codes to internal error codes
        return twilioCode switch
        {
            21211 => MessagingErrorCodes.InvalidRecipient,
            21610 => MessagingErrorCodes.InvalidRecipient,
            21614 => TwilioErrorCodes.InvalidSender,
            21408 => TwilioErrorCodes.InvalidSender,
            20001 => TwilioErrorCodes.InvalidMessage,
            _ => MessagingErrorCodes.SendMessageFailed
        };
    }
}