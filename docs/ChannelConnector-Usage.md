# Connector Implementation Guide

If you are building a custom connector, start from `ChannelConnectorBase` and implement only the provider-specific parts.

## Minimum implementation shape

```csharp
public sealed class MyConnector : ChannelConnectorBase
{
    public MyConnector(IChannelSchema schema) : base(schema) { }

    protected override Task<ConnectorResult<bool>> InitializeCoreAsync(CancellationToken ct)
    {
        // Validate settings, create provider client, authenticate
        SetState(ConnectorState.Connected);
        return Task.FromResult(ConnectorResult<bool>.Success(true));
    }

    protected override Task<ConnectorResult<bool>> TestConnectionCoreAsync(CancellationToken ct)
    {
        // Perform a lightweight provider ping/check
        return Task.FromResult(ConnectorResult<bool>.Success(true));
    }

    protected override async Task<ConnectorResult<MessageResult>> SendMessageCoreAsync(IMessage message, CancellationToken ct)
    {
        // Call provider API and map response to MessageResult
        var id = await SendToProviderAsync(message, ct);
        return ConnectorResult<MessageResult>.Success(new MessageResult(id, MessageStatus.Sent));
    }
}
```

## What the base class already does

- Capability checks
- Connector state checks
- Message validation against schema
- Standardized error/result flow

That means your core methods can stay focused on API translation.

## Recommended error style

Use explicit error codes and short messages:

- `INVALID_CREDENTIALS`
- `RATE_LIMITED`
- `NETWORK_ERROR`
- `PROVIDER_VALIDATION_FAILED`

Avoid generic `ERROR` when possible.

## Optional overrides

Override only what your provider supports:

- `SendMessagesCoreAsync` for bulk
- `ReceiveMessagesCoreAsync` for inbound/webhooks
- `GetMessageStatusCoreAsync` for delivery state
- `GetHealthCoreAsync` for health diagnostics

## Production checklist

- Validate required settings in `InitializeCoreAsync`
- Respect cancellation tokens everywhere
- Dispose provider clients/resources properly
- Add structured logging around send/receive paths
- Unit test success, validation failure, and provider failure paths

## Related docs

- [Channel schema usage](channelschema-usage.md)
- [Channel registry guide](channelregistry-guide.md)
