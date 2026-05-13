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
    private readonly ConnectionSettings _connectionSettings;

    public ExampleRefreshableConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
        : base(schema)
    {
        _connectionSettings = connectionSettings;
    }

    protected override async Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
    {
        var authResult = await AuthenticateAsync(_connectionSettings, cancellationToken);
        return authResult.Successful ? ConnectorResult<bool>.Success(true) : authResult;
    }

    protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ConnectorResult<bool>.Success(true));
    }

    protected override async Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        var result = new SendResult(message.Id, $"refresh-{Guid.NewGuid()}");
        return ConnectorResult<SendResult>.Success(result);
    }

    protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ConnectorResult<StatusInfo>.Success(new StatusInfo("Refreshable Connector Ready")));
    }

    /// <summary>
    /// Public method to demonstrate manual token refresh.
    /// </summary>
    public async Task RefreshAuthenticationIfNeeded()
    {
        if (AuthenticationCredential?.WillExpireSoon(TimeSpan.FromMinutes(5)) == true)
        {
            await RefreshAuthenticationAsync(_connectionSettings);
        }
    }
}
