//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Example connector with custom authentication manager.
/// </summary>
public class ExampleCustomConnector : ChannelConnectorBase
{
    private readonly ConnectionSettings _connectionSettings;

    public ExampleCustomConnector(IChannelSchema schema, ConnectionSettings connectionSettings, IAuthenticationManager authManager)
        : base(schema, authenticationManager: authManager)
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
        var result = new SendResult(message.Id, $"custom-{Guid.NewGuid()}");
        return ConnectorResult<SendResult>.Success(result);
    }

    protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ConnectorResult<StatusInfo>.Success(new StatusInfo("Custom Connector Ready")));
    }
}
