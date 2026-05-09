# Schema Derivation Guide

Derivation lets you create specialized schemas from a base schema without redefining everything.

Typical use cases are tenant plans, department restrictions, or feature tiers.

## Base plus restriction pattern

```csharp
var baseSchema = new ChannelSchema("Twilio", "SMS", "1.0.0")
    .WithCapabilities(
        ChannelCapability.SendMessages |
        ChannelCapability.ReceiveMessages |
        ChannelCapability.BulkMessaging)
    .AddParameter(new ChannelParameter("AccountSid", DataType.String) { IsRequired = true })
    .AddParameter(new ChannelParameter("AuthToken", DataType.String) { IsRequired = true, IsSensitive = true })
    .AddParameter(new ChannelParameter("WebhookUrl", DataType.String))
    .AddContentType(MessageContentType.PlainText)
    .AddContentType(MessageContentType.Media)
    .AllowsMessageEndpoint(EndpointType.PhoneNumber)
    .AllowsMessageEndpoint(EndpointType.Url);

var outboundOnly = new ChannelSchema(baseSchema, "Outbound only")
    .RemoveCapability(ChannelCapability.ReceiveMessages)
    .RemoveParameter("WebhookUrl")
    .RemoveEndpoint(EndpointType.Url)
    .RestrictContentTypes(MessageContentType.PlainText);
```

## Compatibility rule

Derived schemas keep the same logical identity (`Provider/Type/Version`) as their base.

That is why they can still be validated and used as compatible runtime variants.

```csharp
var issues = outboundOnly.ValidateAsRestrictionOf(baseSchema);
if (issues.Any())
    throw new InvalidOperationException("Invalid derived schema");
```

## What you can safely change

- Remove capabilities
- Restrict content types
- Remove or tighten endpoints
- Remove optional parameters
- Update defaults and descriptions
- Add or adjust message properties for your use case

## Practical advice

- Start from a complete base schema, then derive restrictions
- Keep derivation chains shallow (1-3 levels is usually enough)
- Give each derived schema a clear display name
- Validate derived schemas before creating connectors from them

## Related docs

- [Channel schema usage](channelschema-usage.md)
- [Channel registry guide](channelregistry-guide.md)
