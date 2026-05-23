//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Twilio.Exceptions;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging;

/// <summary>
/// Default implementation of <see cref="ITwilioService"/> that wraps the actual Twilio SDK calls.
/// </summary>
    public class TwilioService : ITwilioService
{
    private readonly ITwilioApiClient _client;

    /// <summary>
    /// Constructs a <see cref="TwilioService"/> using the default <see cref="TwilioApiClient"/>.
    /// </summary>
    public TwilioService()
    {
        _client = new TwilioApiClient();
    }

    internal TwilioService(ITwilioApiClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public void Initialize(string accountSid, string authToken)
    {
        _client.Initialize(accountSid, authToken);
    }

    /// <inheritdoc/>
    public async Task<AccountResource?> FetchAccountAsync(string accountSid, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.FetchAccountAsync(accountSid);
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
            return await _client.CreateMessageAsync(options);
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
            return await _client.FetchMessageAsync(messageSid);
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

    /// <summary>
    /// Maps a Twilio API error code to an internal messaging error code.
    /// </summary>
    /// <param name="twilioCode">The Twilio error code to map.</param>
    /// <returns>The corresponding internal error code string.</returns>
    public static string MapTwilioErrorCode(int? twilioCode)
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