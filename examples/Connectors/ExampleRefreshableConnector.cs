//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Example connector that demonstrates token refresh functionality.
/// </summary>
public class ExampleRefreshableConnector : ChannelConnectorBase
{
    public ExampleRefreshableConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
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
        await Task.Delay(10, cancellationToken);
        return new SendResult(message.Id, $"refresh-{Guid.NewGuid()}");
    }

    protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        => Task.FromResult(new StatusInfo("Refreshable Connector Ready"));

    /// <summary>
    /// Public method to demonstrate manual token refresh.
    /// </summary>
    public async Task RefreshAuthenticationIfNeeded()
    {
        if (AuthenticationCredential?.WillExpireSoon(TimeSpan.FromMinutes(5)) == true)
        {
            await RefreshAuthenticationAsync(CancellationToken.None);
        }
    }
}
