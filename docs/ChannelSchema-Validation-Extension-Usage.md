# Validation Extensions

Validation helpers are extension methods on `IChannelSchema`, so they work with `ChannelSchema` and custom schema implementations.

## Main methods

```csharp
IEnumerable<ValidationResult> ValidateConnectionSettings(this IChannelSchema schema, ConnectionSettings settings)
IEnumerable<ValidationResult> ValidateMessageProperties(this IChannelSchema schema, IDictionary<string, object?> properties)
IEnumerable<ValidationResult> ValidateMessage(this IChannelSchema schema, IMessage message)
string GetLogicalIdentity(this IChannelSchema schema)
bool IsCompatibleWith(this IChannelSchema schema, IChannelSchema other)
IEnumerable<ValidationResult> ValidateAsRestrictionOf(this IChannelSchema schema, IChannelSchema target)
```

## Typical usage

```csharp
IChannelSchema schema = GetSchema();

var settingsIssues = schema.ValidateConnectionSettings(settings);
var messageIssues = schema.ValidateMessage(message);

if (settingsIssues.Any() || messageIssues.Any())
{
    // Log and stop before connector calls
}
```

## Why this matters

- You can validate early in service layers, not only inside connectors
- You can use the same checks across different schema implementations
- You get a consistent compatibility model for derived schemas

## Recommendation

Prefer `ValidateMessage` over property-only validation whenever possible, because it also checks message id, endpoints, and content type.

## Related docs

- [Message validation examples](validatemessage-usage-examples.md)
- [Channel schema usage](channelschema-usage.md)
