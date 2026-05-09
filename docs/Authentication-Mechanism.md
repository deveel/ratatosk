# Authentication Mechanism

The framework separates connector logic from auth logic.

In practice, connectors declare auth requirements in schema/config, and authentication providers obtain the effective credential during initialization.

## Main pieces

- `IAuthenticationProvider`: obtains and refreshes credentials
- `AuthenticationManager`: selects provider, caches credentials, handles refresh
- `AuthenticationCredential`: normalized credential shape (api key, token, basic, etc.)
- `ChannelConnectorBase` helpers: `AuthenticateAsync`, `GetAuthenticationHeader`, `GetApiKey`

## Common flow

```csharp
protected override async Task<ConnectorResult<bool>> InitializeCoreAsync(CancellationToken ct)
{
    var auth = await AuthenticateAsync(_settings, ct);
    if (!auth.IsSuccess)
        return auth;

    // Continue connector-specific initialization
    return ConnectorResult<bool>.Success(true);
}
```

## Built-in auth styles

- Direct credentials (api key, token, basic)
- OAuth client credentials
- Firebase service account
- Custom providers you register yourself

## Why it is useful

- Connector code stays focused on provider operations
- Auth behavior is consistent across connectors
- Refresh/caching can be centralized
- Tests are easier because auth can be mocked independently

## Related docs

- [Enhanced authentication configuration](enhanced-authentication-configuration.md)
- [Connector implementation](channelconnector-usage.md)
