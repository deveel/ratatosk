# Channel Schema Usage

`ChannelSchema` is the contract that defines what a connector can do and what it needs to run.

If you get schemas right, message validation and connector behavior become predictable.

## A practical schema

```csharp
var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
    .WithDisplayName("Twilio SMS")
    .WithCapabilities(
        ChannelCapability.SendMessages |
        ChannelCapability.MessageStatusQuery)
    .AddParameter(new ChannelParameter("AccountSid", DataType.String) { IsRequired = true })
    .AddParameter(new ChannelParameter("AuthToken", DataType.String) { IsRequired = true, IsSensitive = true })
    .AddContentType(MessageContentType.PlainText)
    .AllowsMessageEndpoint(EndpointType.PhoneNumber)
    .AddMessageProperty(new MessagePropertyConfiguration("ValidityPeriod", DataType.Integer));
```

## What a schema usually defines

- Identity: provider, channel type, version
- Capabilities: send, receive, status query, bulk, templates, health check
- Parameters: connection values and auth inputs
- Endpoints: allowed sender/receiver endpoint types
- Content types: text, html, media, template, json, multipart
- Message properties: optional per-message knobs (priority, subject, ttl, etc.)

## Validation entry points

```csharp
var settingsIssues = schema.ValidateConnectionSettings(connectionSettings);
var messageIssues = schema.ValidateMessage(message);
```

Both methods return validation results you can log, fail fast on, or surface in UI.

## Strict vs flexible mode

- Default mode is strict: unknown settings/properties are treated as errors
- Flexible mode allows extra keys, which can help during staged rollouts

```csharp
var flexible = new ChannelSchema("Provider", "Type", "1.0.0")
    .WithFlexibleMode();
```

## Tips that age well

- Keep capability flags honest; do not advertise features a connector does not implement
- Mark secrets as `IsSensitive`
- Prefer specific endpoint types over catch-all values
- Add short, useful descriptions to parameters and properties
- Use semantic versions in schemas when behavior changes

## Related docs

- [Schema derivation guide](channelschema-derivation-guide.md)
- [Validation extensions](channelschema-validation-extension-usage.md)
- [Connector implementation](channelconnector-usage.md)
