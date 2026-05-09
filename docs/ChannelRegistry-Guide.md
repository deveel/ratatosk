# Channel Registry Guide

`IChannelRegistry` is the central place where channel ids map to connector types and master schemas.

It is especially useful when you need runtime connector selection or schema variants per tenant/customer.

## Core idea

- Register connector + master schema once
- Optionally create restricted runtime schemas later
- Validate runtime schema against the master before creating connectors

## Registration example

```csharp
services.AddChannelRegistry()
    .RegisterChannel<TwilioSmsConnector>("twilio-sms", twilioMasterSchema)
    .RegisterChannel<SendGridEmailConnector>("sendgrid-email", sendGridMasterSchema);
```

## Runtime restriction example

```csharp
var master = registry.GetMasterSchema("twilio-sms");

var outboundOnly = new ChannelSchema(master, "Outbound only")
    .RemoveCapability(ChannelCapability.ReceiveMessages)
    .RemoveParameter("WebhookUrl");

var issues = registry.ValidateRuntimeSchema("twilio-sms", outboundOnly);
if (issues.Any())
    throw new InvalidOperationException("Invalid runtime schema");

var connector = await registry.CreateConnectorAsync("twilio-sms", outboundOnly);
```

## Discovery helpers

You can query descriptors by capability, provider, channel type, or content type to drive UI and runtime decisions.

## Good practices

- Make master schemas complete and realistic
- Keep channel ids stable and descriptive (`twilio-sms`, `sendgrid-email`)
- Validate every runtime schema before connector creation
- Prefer restriction-based runtime schemas over ad-hoc mutable config objects

## Related docs

- [Schema derivation guide](channelschema-derivation-guide.md)
- [Installation and setup](installation-setup.md)
