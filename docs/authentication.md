---
sidebar_position: 9
---

# Authentication

Every messaging provider requires some form of authentication — an API key, a bearer token, a username and password pair, or an OAuth 2.0 client credentials flow. If every connector handled auth internally, each would duplicate logic for token refresh, credential caching, and error handling. Authentication would be inconsistent across connectors, and adding a new auth scheme would require modifying every connector.

The framework separates authentication from connector logic:

- **Connectors declare** what authentication they support in their schema via `AuthenticationConfiguration`
- **Providers** (implementing `IAuthenticationProvider`) handle credential acquisition and management
- **`ChannelConnectorBase`** delegates to a central `IAuthenticationManager` and auto-authenticates during initialization
- **New auth schemes** can be added without modifying existing connectors

## Core types

### AuthenticationScheme

`AuthenticationScheme` is an extensible record type that identifies an authentication mechanism. It replaces the old `AuthenticationType` sealed enum.

```csharp
public record AuthenticationScheme(string Name)
{
    public static AuthenticationScheme None { get; }
    public static AuthenticationScheme ApiKey { get; }
    public static AuthenticationScheme Bearer { get; }
    public static AuthenticationScheme Basic { get; }
    public static AuthenticationScheme OAuthClientCredentials { get; }
    public static AuthenticationScheme Certificate { get; }
    public static AuthenticationScheme Digest { get; }
    public static AuthenticationScheme Custom { get; }

    // Create custom schemes:
    public static AuthenticationScheme Of(string name) => new(name);
}
```

Built-in schemes cover the most common authentication patterns:

| Scheme | Typical use |
|---|---|
| `None` | Public endpoints, no auth required |
| `ApiKey` | SendGrid, generic REST APIs |
| `Bearer` | Facebook (Page Access Token), Telegram (Bot Token) |
| `Basic` | Twilio (AccountSid + AuthToken), SMTP |
| `OAuthClientCredentials` | Server-to-server OAuth 2.0 |
| `Certificate` | Firebase (service account key) |
| `Digest` | RFC 7616 digest authentication |
| `Custom` | Provider-specific schemes |

Custom schemes are created with `AuthenticationScheme.Of("my-scheme")` — no enum modification needed.

### AuthenticationField

Describes a single field that an authentication configuration expects in `ConnectionSettings`. It is purely descriptive — validation is handled by providers.

```csharp
public sealed class AuthenticationField
{
    public string FieldName { get; }              // Connection settings key
    public DataType DataType { get; }             // Expected type
    public bool IsSensitive { get; set; }         // Redact in logs
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public IList<object>? AllowedValues { get; set; }
    public string? AuthenticationRole { get; set; }  // "principal", "credential", etc.
}
```

The `AuthenticationRole` property tells providers how to interpret the field:

| Role | Meaning | Example fields |
|---|---|---|
| `"principal"` | The primary identifier | ApiKey, Token, Username, AccountSid, Certificate |
| `"credential"` | The corresponding secret | Password, AuthToken, ClientSecret |
| (other) | Auxiliary, treated as optional | ProjectId, CertificatePassword |

### AuthenticationConfiguration

Describes the set of fields required by a particular scheme. It is purely descriptive — providers use the field definitions to extract values from `ConnectionSettings`.

```csharp
public class AuthenticationConfiguration
{
    public AuthenticationScheme Scheme { get; }
    public string DisplayName { get; }
    public IList<AuthenticationField> Fields { get; }

    // Fluent building:
    public AuthenticationConfiguration WithField(string name, DataType type,
        Action<AuthenticationField>? configure = null);

    // Role-based queries for providers:
    public IEnumerable<AuthenticationField> GetFieldsByRole(string role);
    public IEnumerable<string> GetAllFieldNames();

    // Role-aware matching:
    public bool IsSatisfiedBy(ConnectionSettings settings);
}
```

#### IsSatisfiedBy

The `IsSatisfiedBy` method checks whether a `ConnectionSettings` instance has enough values to satisfy this configuration, using role-aware logic:

- If the config has both `"principal"` and `"credential"` role fields, at least one of each must be present
- If the config has only `"principal"` fields, at least one must be present
- All non-principal, non-credential fields must be present

This is used by the schema validation and by `ChannelConnectorBase.AuthenticateAsync()` to find the correct auth configuration for a given set of connection settings.

### AuthenticationCredential

Holds the result of a successful authentication.

```csharp
public class AuthenticationCredential
{
    public AuthenticationScheme Scheme { get; }
    public string Value { get; }                // Primary secret (token, key, etc.)
    public DateTime? ExpiresAt { get; }
    public DateTime ObtainedAt { get; }
    public Dictionary<string, object?> Properties { get; }
    public bool IsExpired { get; }

    // Factory methods:
    public static AuthenticationCredential ForBearerToken(string token,
        DateTime? expiresAt = null, string? tokenType = null);
    public static AuthenticationCredential ForApiKey(string apiKey);
    public static AuthenticationCredential ForBasic(string username, string password);
    public static AuthenticationCredential ForClientCredentials(string accessToken,
        DateTime? expiresAt = null, string? tokenType = null,
        string? refreshToken = null);
    public static AuthenticationCredential ForCertificate(string certificateData,
        string? password = null);
}
```

### AuthenticationResult

Wraps the outcome of an authentication operation.

```csharp
public class AuthenticationResult
{
    public bool IsSuccessful { get; }
    public AuthenticationCredential? Credential { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public DateTime Timestamp { get; }
    public Dictionary<string, object?> AdditionalData { get; }

    public static AuthenticationResult Success(AuthenticationCredential credential);
    public static AuthenticationResult Failure(string errorMessage,
        string? errorCode = null);
}
```

## Architecture

```
Connector.InitializeAsync()
  └─ Auto-authenticate (base class does this first)
       └─ AuthenticateAsync()
            └─ IAuthenticationManager.AuthenticateAsync()
                 ├─ Find matching config via IsSatisfiedBy()
                 ├─ Find matching provider via CanHandle()
                 └─ IAuthenticationProvider.ObtainCredentialAsync(
                        ConnectionSettings,
                        AuthenticationConfiguration)   // ← config tells provider what fields to use
                     → AuthenticationResult

Connector.InitializeConnectorAsync()
  └─ AuthenticationCredential is available via AuthenticationCredential property
  └─ GetAuthenticationHeader()  → "Bearer <token>" | "Basic <base64>" | null
  └─ GetApiKey()                → raw API key | null
```

## Lifecycle

1. **Schema declares** what auth methods the connector supports via `AuthenticationConfiguration`
2. **Connector initializes** — `ChannelConnectorBase.InitializeAsync()` automatically calls `AuthenticateAsync()` if the schema has auth configurations
3. **Config selected** — iterates schema configs, picks the first where `IsSatisfiedBy(ConnectionSettings)` returns true
4. **Provider matched** — `IAuthenticationManager` finds a registered `IAuthenticationProvider` whose `CanHandle(config)` returns true
5. **Credential obtained** — provider reads fields from `ConnectionSettings` (using the config's field definitions) and returns an `AuthenticationCredential`
6. **Credential cached** — the manager caches the credential in memory
7. **Connector uses credential** — `AuthenticationCredential.Value`, `GetAuthenticationHeader()`, or `GetApiKey()` provide the secret for API calls
8. **Expired credentials refresh** — the manager detects expiration and either refreshes via the provider or obtains a new credential

## Schema auth configuration

### Adding authentication to a schema

```csharp
// Via convenience method (auto-selects field aliases for the scheme):
new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
    .AddAuthenticationScheme(AuthenticationScheme.Bearer)

// Via explicit configuration (recommended — precise field names):
new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
    .AddAuthenticationConfiguration(
        new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "My API Key")
            .WithField("ApiKey", DataType.String, f =>
            {
                f.AuthenticationRole = "principal";
                f.IsSensitive = true;
            }))

// Multiple options — the first matching one is selected:
new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
    .AddAuthenticationConfiguration(
        new AuthenticationConfiguration(AuthenticationScheme.Basic, "Standard Auth")
            .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
            .WithField("Password", DataType.String, f =>
            {
                f.AuthenticationRole = "credential";
                f.IsSensitive = true;
            }))
    .AddAuthenticationConfiguration(
        new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Fallback Key")
            .WithField("ApiKey", DataType.String, f =>
            {
                f.AuthenticationRole = "principal";
                f.IsSensitive = true;
            }))
```

### Auto-registration of principal fields

When using `AddAuthenticationConfiguration()` with an explicit config, any field with `AuthenticationRole = "principal"` is automatically added as an optional `ChannelParameter` in the schema if not already defined. This ensures auth fields are recognized by strict-mode validation without requiring duplicate `AddParameter` calls.

`AddAuthenticationScheme()` (which uses flexible defaults) does NOT auto-register fields, avoiding parameter list pollution from alias fields that the connector does not actually use.

### Default field aliases per scheme

When `AddAuthenticationScheme(AuthenticationScheme.Xxx)` is used, the builder generates sensible default fields:

| Scheme | Principal fields | Credential fields |
|---|---|---|
| `Basic` | Username, AccountSid, User, ClientId | Password, AuthToken, Pass, ClientSecret |
| `ApiKey` | ApiKey, Key, AccessKey | — |
| `Bearer` | Token, AccessToken, BearerToken, AuthToken | — |
| `OAuthClientCredentials` | ClientId | ClientSecret |
| `Certificate` | Certificate, CertificatePath, CertificateThumbprint, PfxFile | PfxPassword, CertificatePassword |
| `Custom` | CustomAuth, AuthenticationData, Credentials, AuthConfig, SecretKey, PrivateKey, Signature, Hash | — |

For production connectors, use `AddAuthenticationConfiguration()` with explicit fields — it avoids ambiguity and provides better documentation.

## Built-in providers

### ApiKeyAuthenticationProvider

Extracts an API key from connection settings using fields with `"principal"` role. Returns `AuthenticationCredential.ForApiKey(value)`.

```csharp
// Schema:
new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
    .WithField("ApiKey", DataType.String, f =>
    {
        f.AuthenticationRole = "principal";
        f.IsSensitive = true;
    })

// ConnectionSettings:
settings.SetParameter("ApiKey", "sk-1234...");

// Credential value:
credential.Value        // "sk-1234..."
credential.Scheme       // AuthenticationScheme.ApiKey
```

### BearerTokenAuthenticationProvider

Extracts a bearer token from connection settings using fields with `"principal"` role. Supports optional `TokenType` and `ExpiresAt` parameters.

```csharp
// Schema:
new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bearer Token")
    .WithField("AccessToken", DataType.String, f =>
    {
        f.AuthenticationRole = "principal";
        f.IsSensitive = true;
    })

// ConnectionSettings:
settings.SetParameter("AccessToken", "eyJ...");
settings.SetParameter("TokenType", "Bearer");
settings.SetParameter("ExpiresAt", "2025-12-31T23:59:59Z");

// Credential value:
credential.Value        // "eyJ..."
credential.Scheme       // AuthenticationScheme.Bearer
```

### BasicAuthenticationProvider

Extracts a username/password pair from connection settings. Looks for fields with `"principal"` role (e.g. Username, AccountSid) and pairs them with fields with `"credential"` role (e.g. Password, AuthToken), trying each combination until a complete pair is found.

```csharp
// Schema (Twilio-style):
new AuthenticationConfiguration(AuthenticationScheme.Basic, "Twilio Auth")
    .WithField("AccountSid", DataType.String, f => f.AuthenticationRole = "principal")
    .WithField("AuthToken", DataType.String, f =>
    {
        f.AuthenticationRole = "credential";
        f.IsSensitive = true;
    });

// ConnectionSettings:
settings.SetParameter("AccountSid", "ACxxx...");
settings.SetParameter("AuthToken", "auth123...");

// Credential value:
credential.Value        // Base64("ACxxx...:auth123...")
credential.Scheme       // AuthenticationScheme.Basic
credential.Properties["Username"]   // "ACxxx..."
credential.Properties["Password"]   // "auth123..."
```

### ClientCredentialsAuthenticationProvider

Implements the OAuth 2.0 Client Credentials flow. Reads ClientId, ClientSecret, and TokenEndpoint from connection settings, POSTs to the token endpoint, and caches the resulting access token.

```csharp
// Schema:
new AuthenticationConfiguration(
    AuthenticationScheme.OAuthClientCredentials, "OAuth 2.0")
    .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
    .WithField("ClientSecret", DataType.String, f =>
    {
        f.AuthenticationRole = "credential";
        f.IsSensitive = true;
    })
    .WithField("TokenEndpoint", DataType.String, _ => { })
    .WithField("Scope", DataType.String, _ => { });
```

The provider:
1. Reads `ClientId` and `ClientSecret` from connection settings
2. POSTs `grant_type=client_credentials` to the token endpoint
3. Parses the JSON response for `access_token`, `expires_in`, `token_type`
4. Supports `refresh_token` if present in the response
5. Caches the resulting credential and refreshes when expired

The credential is created with `AuthenticationCredential.ForClientCredentials()` which uses `AuthenticationScheme.Bearer` (since a client credentials flow still produces a bearer token), with `Properties["GrantType"] = "client_credentials"`.

## Using auth in a connector

Connectors no longer need to call `AuthenticateAsync()` manually — the base class does it automatically during `InitializeAsync()`:

```csharp
protected override async ValueTask InitializeConnectorAsync(CancellationToken ct)
{
    // AuthenticationCredential is already populated by the base class.
    // Access the credential value:
    var token = AuthenticationCredential?.Value;
    var scheme = AuthenticationCredential?.Scheme;

    // Use helpers for common auth patterns:
    var authHeader = GetAuthenticationHeader();
    // Returns: "Bearer eyJ..." | "Basic base64..." | null

    var apiKey = GetApiKey();
    // Returns raw API key value (null if not ApiKey scheme)

    _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
}
```

If auto-authentication fails (e.g., missing connection settings), the base class logs a warning but does not prevent initialization — the connector can still authenticate later or handle the error:

```csharp
protected override async ValueTask InitializeConnectorAsync(CancellationToken ct)
{
    // Fallback: manually authenticate with specific config
    var result = await AuthenticateAsync(ct);
    if (!result.IsSuccess())
        throw new ConnectorException("AUTH_FAILED",
            "Unable to authenticate with the provided settings");

    var credential = AuthenticationCredential!;
    // ...
}
```

### Refresh

```csharp
var refreshResult = await RefreshAuthenticationAsync(ct);
```

Refreshes the current credential if it has expired (or will expire within 5 minutes). Falls back to `AuthenticateAsync()` if no credential exists.

## Credential caching

`AuthenticationManager` caches credentials by `(connectionSettings, authConfiguration)` tuple. The cache key is built from the scheme name and parameter values (using hash codes). When `AuthenticateAsync` is called:

1. Check cache for a non-expired credential
2. If found and not about to expire, return directly
3. If expired, call `RefreshCredentialAsync` on the provider
4. If not cached, call `ObtainCredentialAsync`

```csharp
// Clear all cached credentials
authenticationManager.ClearCache();

// Invalidate a specific credential
authenticationManager.InvalidateCredential(settings, authConfig);
```

## Custom authentication provider

Implement `IAuthenticationProvider` (or extend `AuthenticationProviderBase`) for custom auth flows:

```csharp
public class HmacSignatureProvider : AuthenticationProviderBase
{
    public HmacSignatureProvider()
        : base(AuthenticationScheme.Of("hmac-sha256"), "HMAC-SHA256 Signature")
    {
    }

    public override Task<AuthenticationResult> ObtainCredentialAsync(
        ConnectionSettings settings,
        AuthenticationConfiguration configuration,
        CancellationToken ct)
    {
        var apiKey = GetStringParameter(settings, "ApiKey");
        var secretKey = GetStringParameter(settings, "SecretKey");

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secretKey))
            return Task.FromResult(
                Failure("ApiKey and SecretKey are required", "MISSING_CREDENTIALS"));

        var credential = new AuthenticationCredential(
            AuthenticationScheme.Of("hmac-sha256"),
            $"{apiKey}:{secretKey}");

        credential.Properties["ApiKey"] = apiKey;
        return Task.FromResult(Success(credential));
    }
}
```

### Schema for custom provider

```csharp
new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
    .AddAuthenticationConfiguration(
        new AuthenticationConfiguration(
            AuthenticationScheme.Of("hmac-sha256"), "HMAC Signature")
            .WithField("ApiKey", DataType.String, f =>
            {
                f.AuthenticationRole = "principal";
                f.IsSensitive = true;
            })
            .WithField("SecretKey", DataType.String, f =>
            {
                f.AuthenticationRole = "credential";
                f.IsSensitive = true;
            }));
```

### Registration

```csharp
var authManager = serviceProvider.GetRequiredService<IAuthenticationManager>();
authManager.RegisterProvider(new HmacSignatureProvider());
```

Or provide a custom `IAuthenticationManager` implementation.

## Provider matching

When `IAuthenticationManager.AuthenticateAsync()` is called, it:

1. Finds the `AuthenticationConfiguration` in the schema that satisfies the connection settings (`IsSatisfiedBy`)
2. Iterates registered providers, calling `CanHandle(configuration)` on each
3. Delegates to the first matching provider

The default `AuthenticationProviderBase.CanHandle()` matches on `Scheme` equality:

```csharp
public virtual bool CanHandle(AuthenticationConfiguration configuration)
{
    return configuration.Scheme == Scheme;
}
```

Override `CanHandle()` for providers that handle multiple schemes or need additional checks:

```csharp
public override bool CanHandle(AuthenticationConfiguration configuration)
{
    return configuration.Scheme == AuthenticationScheme.Certificate ||
           (configuration.Scheme == AuthenticationScheme.Custom &&
            configuration.GetAllFieldNames()
                .Any(f => f.Contains("ServiceAccount")));
}
```

## Schema validation integration

The `ChannelSchemaExtensions.ValidateConnectionSettings()` method uses `IsSatisfiedBy()` to check that at least one auth configuration can be fulfilled by the provided settings. If none matches, a `ValidationResult` is added to the error list.

Auth field names are also added to the "known parameters" set for strict-mode validation, so they do not produce "Unknown parameter" errors even when not declared as explicit schema parameters.

## Migration from AuthenticationType

The old `AuthenticationType` enum and `AuthenticationConfigurations` static factory class have been removed:

| Before | After |
|---|---|
| `AuthenticationType.ApiKey` | `AuthenticationScheme.ApiKey` |
| `AuthenticationType.Token` | `AuthenticationScheme.Bearer` |
| `AuthenticationType.ClientCredentials` | `AuthenticationScheme.OAuthClientCredentials` |
| `AuthenticationConfigurations.ApiKeyAuthentication()` | `new AuthenticationConfiguration(...).WithField(...)` |
| `AuthenticationConfigurations.FlexibleBasicAuthentication()` | `new AuthenticationConfiguration(...).WithField(...)` × N fields |
| `config.RequiredFields` | `config.Fields` |
| `config.OptionalFields` | `config.Fields` |
| `config.IsSatisfiedBy(settings)` | `config.IsSatisfiedBy(settings)` (re-added with role-aware logic) |
| `config.Validate(settings)` | *removed* — providers handle validation |
| `credential.CredentialValue` | `credential.Value` |
| `credential.AuthenticationType` | `credential.Scheme` |
| `AuthenticationCredential.CreateToken(...)` | `AuthenticationCredential.ForBearerToken(...)` |
| `AuthenticationCredential.CreateApiKey(...)` | `AuthenticationCredential.ForApiKey(...)` |
| `AuthenticationCredential.CreateBasic(...)` | `AuthenticationCredential.ForBasic(...)` |
| `DirectCredentialAuthenticationProvider` | `ApiKeyAuthenticationProvider`, `BearerTokenAuthenticationProvider`, `BasicAuthenticationProvider` |
