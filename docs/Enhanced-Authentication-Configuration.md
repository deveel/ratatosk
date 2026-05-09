# Enhanced Authentication Configuration

This auth model lets you define exactly which fields are required for each authentication strategy.

It is useful when provider requirements are stricter than generic `AuthenticationType` defaults.

## Explicit mapping example

```csharp
var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
    .AddAuthenticationConfiguration(
        AuthenticationConfigurations.CustomBasicAuthentication(
            usernameField: "AccountSid",
            passwordField: "AuthToken",
            name: "Twilio Basic"));
```

Now validation checks exactly `AccountSid` and `AuthToken`.

## Flexible alternatives example

```csharp
var schema = new ChannelSchema("Api", "Generic", "1.0.0")
    .AddAuthenticationConfiguration(
        AuthenticationConfigurations.FlexibleApiKeyAuthentication("ApiKey", "Key", "AccessKey"));
```

Any one of those fields can satisfy auth validation.

## Legacy compatibility

`AddAuthenticationType(...)` still works. The enhanced model is just more explicit and easier to document.

## When to use which

- Use enhanced auth config when field names are provider-specific or non-standard
- Use legacy auth type config for quick setups and generic connectors

## Related docs

- [Authentication mechanism](authentication-mechanism.md)
- [Channel schema usage](channelschema-usage.md)
