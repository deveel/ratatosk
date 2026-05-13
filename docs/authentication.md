# Authentication

Every provider requires some form of authentication — an API key, a bearer token, a username and password, or an OAuth 2.0 client credentials flow. If every connector handled auth internally, each one would duplicate the logic for token refresh, credential caching, and error handling. Authentication would be inconsistent across connectors, and adding a new auth scheme would require modifying every connector that uses it.

The framework separates authentication from connector logic. Connectors declare what authentication they support in their schema (via `AuthenticationConfiguration`), and `IAuthenticationProvider` implementations handle the actual credential acquisition and management. The `ChannelConnectorBase` provides `AuthenticateAsync()`, `GetAuthenticationHeader()`, and `GetApiKey()` methods that delegate to a central `IAuthenticationManager`. This means:
- Connector code stays focused on the provider API — it does not handle token refresh, credential caching, or auth flow
- Authentication behavior is consistent across all connectors — same caching, same refresh logic, same error format
- Tests can mock authentication independently of connector logic
- New auth providers can be added without modifying existing connectors

The framework separates connector logic from authentication. Connectors declare what authentication they support in their schema; `IAuthenticationProvider` implementations obtain and manage credentials.

This separation means:
- Connector code stays focused on the provider API — it doesn't handle token refresh, credential caching, or auth flow
- Authentication behavior is consistent across connectors
- Tests can mock authentication independently of connector logic
- New auth providers can be added without modifying existing connectors

## Architecture

```
Connector.InitializeAsync()
  └─ AuthenticateAsync()               // ChannelConnectorBase method
       └─ IAuthenticationManager
            ├─ Find matching provider   // Matches AuthenticationConfiguration
            └─ IAuthenticationProvider
                 └─ ObtainCredentialAsync(ConnectionSettings)
                      → AuthenticationResult

Connector.SendMessageAsync()
  └─ GetAuthenticationHeader()          // Returns "Bearer <token>" from cached credential
  └─ GetApiKey()                        // Returns raw API key value
```

## Authentication lifecycle

1. **Schema declares** what auth methods the connector supports via `AuthenticationConfiguration`
2. **Connector initializes** — `AuthenticateAsync()` is called, which uses `IAuthenticationManager` to find a matching `IAuthenticationProvider`
3. **Provider obtains credential** — the provider reads connection settings and returns an `AuthenticationCredential`
4. **Credential is cached** — the manager caches the credential for reuse
5. **API calls use credential** — `GetAuthenticationHeader()` or `GetApiKey()` return the credential value
6. **Expired credentials refresh** — the manager detects expiration and auto-refreshes

## Authentication types

```csharp
public enum AuthenticationType
{
    None,               // No authentication required
    ApiKey,             // Static API key
    Basic,              // Username + password (Basic auth)
    Token,              // Bearer token
    ClientCredentials,  // OAuth 2.0 client credentials
    Certificate,        // Client certificate
    Custom              // Custom authentication flow
}
```

## Built-in providers

### DirectCredentialAuthenticationProvider

Handles static credentials (API keys, tokens, basic auth). This is the most common provider — used by Twilio, SendGrid, Telegram, and Facebook connectors.

```csharp
// API key auth
AuthenticationConfigurations.ApiKeyAuthentication("ApiKey")

// Bearer token auth
AuthenticationConfigurations.TokenAuthentication("AuthToken")

// Basic auth (username + password fields in ConnectionSettings)
AuthenticationConfigurations.BasicAuthentication("Username", "Password")

// Flexible API key — any of the named fields can satisfy auth
AuthenticationConfigurations.FlexibleApiKeyAuthentication("Key", "AccessKey", "ApiKey")

// Custom basic auth — maps provider-specific field names
AuthenticationConfigurations.CustomBasicAuthentication(
    usernameField: "AccountSid",
    passwordField: "AuthToken",
    name: "Twilio Basic")
```

### ClientCredentialsAuthenticationProvider

OAuth 2.0 client credentials flow. Used when the provider requires a token endpoint:

```csharp
AuthenticationConfigurations.ClientCredentialsAuthentication(
    clientIdField: "ClientId",
    clientSecretField: "ClientSecret",
    tokenEndpoint: "https://auth.provider.com/oauth/token")
```

The provider:
1. Reads `ClientId` and `ClientSecret` from connection settings
2. POSTs to the token endpoint
3. Caches the resulting `AuthenticationCredential`
4. Refreshes when the token expires

### FirebaseServiceAccountAuthenticationProvider

Firebase-specific JWT-based authentication using a Google service account key:

```csharp
AuthenticationConfigurations.FirebaseServiceAccountAuthentication(
    projectIdField: "ProjectId",
    serviceAccountKeyField: "ServiceAccountKey")
```

Generates a signed JWT using the service account's private key, which is sent to Firebase as a bearer token.

## Schema auth configuration

Connectors declare supported auth in their schema:

```csharp
var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
    // Single auth method
    .AddAuthenticationConfiguration(
        AuthenticationConfigurations.CustomBasicAuthentication(
            usernameField: "AccountSid",
            passwordField: "AuthToken",
            name: "Twilio Basic"));

// Multiple fallback options — the manager tries each in order
var schema = new ChannelSchema("Api", "Generic", "1.0.0")
    .AddAuthenticationConfiguration(
        AuthenticationConfigurations.ApiKeyAuthentication("PrimaryKey"))
    .AddAuthenticationConfiguration(
        AuthenticationConfigurations.ApiKeyAuthentication("SecondaryKey"));
```

The `IAuthenticationManager` iterates through the schema's auth configurations until one succeeds.

## Using auth in a connector

```csharp
protected override async ValueTask InitializeConnectorAsync(CancellationToken ct)
{
    // AuthenticateAsync is provided by ChannelConnectorBase.
    // It calls IAuthenticationManager.AuthenticateAsync().
    var authResult = await AuthenticateAsync(ct);

    if (authResult.IsFailure)
    {
        throw new InvalidOperationException(
            $"Authentication failed: {authResult.Error?.ErrorMessage}");
    }

    // After successful auth, use helpers to get credential values:
    var authHeader = GetAuthenticationHeader();
    // Returns: "Bearer eyJ..." or "Basic base64..." or "ApiKey sk-..."

    var apiKey = GetApiKey();
    // Returns the raw API key value (null if credential is not an API key)

    _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
}
```

### Refresh

```csharp
// Force re-authentication
var refreshResult = await RefreshAuthenticationAsync(ct);
```

## AuthenticationResult

```csharp
var result = await AuthenticateAsync(ct);

if (result.IsSuccessful)
{
    var credential = result.Credential;
    // credential.CredentialValue — the actual secret/token
    // credential.ExpiresAt — when it expires (null for static credentials)
    // credential.AuthenticationType — how it was obtained
}
else
{
    Console.WriteLine($"Auth failed: {result.ErrorMessage}");
    // result.ErrorCode — machine-readable code
}
```

### Factory methods

```csharp
AuthenticationResult.Success(credential);
AuthenticationResult.Failure("INVALID_CREDENTIALS", "API key is invalid");
```

## AuthenticationCredential

```csharp
var apiKey = AuthenticationCredential.CreateApiKey("sk-...");
var token = AuthenticationCredential.CreateToken("eyJ...", expiresAt: DateTime.UtcNow.AddHours(1), tokenType: "Bearer");
var basic = AuthenticationCredential.CreateBasic("username", "password");
```

Properties:

| Property | Description |
|---|---|
| `AuthenticationType` | How the credential was obtained |
| `CredentialValue` | The secret value (key, token, password) |
| `ObtainedAt` | When the credential was obtained |
| `ExpiresAt` | Expiration time (null = never expires) |
| `IsExpired` | `true` if current time > `ExpiresAt` |
| `Properties` | Additional metadata dictionary |

## Custom authentication provider

Implement `IAuthenticationProvider` (or extend `AuthenticationProviderBase`) for custom auth flows:

```csharp
public class CustomAuthProvider : AuthenticationProviderBase
{
    public override AuthenticationType AuthenticationType => AuthenticationType.Custom;

    public override async Task<AuthenticationResult> ObtainCredentialAsync(
        ConnectionSettings settings, CancellationToken ct)
    {
        var clientId = settings.GetParameter("ClientId");
        var clientSecret = settings.GetParameter("ClientSecret");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            return AuthenticationResult.Failure(
                "MISSING_CREDENTIALS",
                "ClientId and ClientSecret are required");

        // Custom auth flow — e.g., call an external identity provider
        var token = await FetchTokenAsync(clientId, clientSecret, ct);

        var credential = AuthenticationCredential.CreateToken(
            "CustomProvider", token, expiresAt: DateTime.UtcNow.AddMinutes(30));

        return AuthenticationResult.Success(credential);
    }

    public override bool CanHandle(AuthenticationConfiguration configuration)
    {
        return configuration.AuthenticationType == AuthenticationType.Custom
            && configuration.RequiredFields.Any(f => f.Name == "ClientId");
    }
}
```

### Registering a custom provider

```csharp
// During DI setup
var authManager = serviceProvider.GetRequiredService<IAuthenticationManager>();
authManager.RegisterProvider(new CustomAuthProvider());
```

Or provide a custom `IAuthenticationManager` implementation entirely.

## Credential caching

`AuthenticationManager` caches credentials by `(connectionSettings, authConfiguration)` tuple. When `AuthenticateAsync` is called:

1. Check cache for a non-expired credential
2. If found, return it directly
3. If expired, call `RefreshCredentialAsync` on the provider
4. If not cached, call `ObtainCredentialAsync`

### Cache management

```csharp
// Clear all cached credentials
authenticationManager.ClearCache();

// Invalidate a specific credential (forces re-authentication)
authenticationManager.InvalidateCredential(settings, authConfig);
```
