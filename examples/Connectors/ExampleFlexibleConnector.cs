//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Example flexible connector that supports multiple authentication methods.
/// </summary>
public class ExampleFlexibleConnector : ChannelConnectorBase
{
    public ExampleFlexibleConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
        : base(schema, connectionSettings)
    {
    }

    protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
    {
        var authResult = await AuthenticateAsync(cancellationToken);
        if (!authResult.IsSuccess())
            throw new ConnectorException(
                authResult.Error?.Code ?? ConnectorErrorCodes.AuthenticationFailed,
                authResult.Error?.Domain ?? Schema.ChannelType,
                authResult.Error?.Message ?? "Authentication failed");
    }

    protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
    {
        var authHeader = GetAuthenticationHeader() ?? $"ApiKey {GetApiKey()}";

        await Task.Delay(10, cancellationToken);

        return new SendResult(message.Id, $"flexible-{Guid.NewGuid()}");
    }

    protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        => Task.FromResult(new StatusInfo("Flexible Connector Ready"));
}
