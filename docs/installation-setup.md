# Installation and Setup

This guide helps you pick the right packages and wire the framework into a typical ASP.NET Core app.

## Package selection

Install only what you need.

```bash
dotnet add package Deveel.Messaging.Abstractions
dotnet add package Deveel.Messaging.Connectors
dotnet add package Deveel.Messaging.Connector.Twilio
```

If you are implementing your own connector, also add:

```bash
dotnet add package Deveel.Messaging.Connector.Abstractions
```

## Register connectors in DI

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddChannelRegistry()
    .RegisterConnector<TwilioSmsConnector>()
    .RegisterConnector<SendGridEmailConnector>();
```

## Resolve by channel id

```csharp
public class Notifications(IChannelRegistry registry)
{
    public async Task SendAsync(IMessage message, CancellationToken ct)
    {
        var connector = await registry.GetConnectorAsync("twilio-sms", ct);
        var result = await connector.SendMessageAsync(message, ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error?.Description ?? "Send failed");
    }
}
```

## Settings pattern

Keep provider credentials in configuration and map them into `ConnectionSettings`.

```csharp
var settings = new ConnectionSettings()
    .SetParameter("AccountSid", configuration["Twilio:AccountSid"])
    .SetParameter("AuthToken", configuration["Twilio:AuthToken"]);
```

```json
{
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "..."
  }
}
```

## Verify

```bash
dotnet restore
dotnet build
```

## Next reads

- [Quick start](quick-start.md)
- [Channel registry guide](channelregistry-guide.md)
- [Connector index](connectors/README.md)



