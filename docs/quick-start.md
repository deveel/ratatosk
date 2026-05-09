# Quick Start

If you just want to prove the framework works in your app, this is the shortest path.

## 1) Install one connector package

Pick the channel you want to test first.

```bash
dotnet add package Deveel.Messaging.Connector.Twilio
```

## 2) Create a connector and send one message

```csharp
using Deveel.Messaging;
using Deveel.Messaging.Connector.Twilio;

var settings = new ConnectionSettings()
    .SetParameter("AccountSid", "your-account-sid")
    .SetParameter("AuthToken", "your-auth-token");

var connector = new TwilioSmsConnector(TwilioChannelSchemas.SimpleSms, settings);
await connector.InitializeAsync(CancellationToken.None);

var message = Message.Create()
    .From(PhoneEndpoint.Create("+15550001111"))
    .To(PhoneEndpoint.Create("+15550002222"))
    .WithText("Hello from Deveel Messaging")
    .Build();

var result = await connector.SendMessageAsync(message, CancellationToken.None);
Console.WriteLine(result.IsSuccess ? "Sent" : result.Error?.Description);
```

## 3) Move to DI when you are ready

```csharp
builder.Services
    .AddChannelRegistry()
    .RegisterConnector<TwilioSmsConnector>();
```

You can then resolve connectors from `IChannelRegistry` and keep your business services provider-agnostic.

## What to read next

- [Installation and setup](installation-setup.md)
- [Channel schema usage](channelschema-usage.md)
- [Connector index](connectors/README.md)




