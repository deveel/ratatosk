# Advanced Configuration

The basic patterns — register a connector, build a message, send it — cover the common cases. Production deployments introduce additional concerns: keeping credentials secure, isolating tenants from each other, monitoring connector health, understanding performance characteristics, and testing thoroughly without sending real messages.

This section covers these production patterns. Each pattern is independent — apply the ones that match your deployment context.

## Security

### Credential management

Never store secrets in source code. Use environment variables, user secrets (development), or a vault (production):

```csharp
// appsettings.json — use placeholders, not real values
{
  "Twilio": {
    "AccountSid": "",
    "AuthToken": ""
  }
}

// Environment variables override at runtime
export Twilio__AccountSid="AC..."
export Twilio__AuthToken="..."
```

### Sensitive parameter redaction

Mark schema parameters as `IsSensitive` — the framework redacts their values in logs:

```csharp
new ChannelParameter("AuthToken", DataType.String)
{
    IsRequired = true,
    IsSensitive = true
};
```

When logging, sensitive parameter values appear as `"***"` instead of the actual value.

### Webhook signature validation

Inbound webhooks from providers include cryptographic signatures. Always validate them:

- **Twilio**: validate `X-Twilio-Signature` header using your auth token
- **Telegram**: set `SecretToken` and validate `X-Telegram-Bot-Api-Secret-Token`
- **Facebook**: validate `X-Hub-Signature-256` using your app secret
- **SendGrid**: validate `X-Twilio-Email-Event-Webhook-Signature`

### Multi-tenant isolation

Use named connectors with per-tenant settings:

```csharp
services.AddMessaging()
    .AddConnector<TwilioSmsConnector>($"tenant-{tenant.Id}", cfg => cfg
        .WithSettings($"Tenants:{tenant.Id}:Twilio"));
```

Each tenant gets its own connector instance with isolated credentials and settings.

## Multi-tenancy

### Schema derivation per tenant

```csharp
public class TenantConnectorFactory
{
    private readonly IChannelSchemaRegistry _registry;
    private readonly ITenantStore _tenants;

    public async Task<IChannelConnector> CreateForTenantAsync(string tenantId)
    {
        var tenant = await _tenants.GetAsync(tenantId);
        var master = _registry.FindSchema("Twilio", "SMS");

        var tenantSchema = new ChannelSchema(master, $"Tenant {tenantId}")
            .UpdateParameter("WebhookUrl", p =>
                p.DefaultValue = tenant.WebhookUrl);

        var settings = new ConnectionSettings()
            .SetParameter("AccountSid", tenant.AccountSid)
            .SetParameter("AuthToken", tenant.AuthToken);

        var connector = new TwilioSmsConnector(tenantSchema, settings);
        await connector.InitializeAsync(CancellationToken.None);
        return connector;
    }
}
```

### Runtime schema selection

```csharp
var master = registry.FindSchema("Twilio", "SMS");

// Tier-based schema restriction
var schema = plan switch
{
    "basic" => new ChannelSchema(master, "Basic")
        .RemoveCapability(ChannelCapability.MediaAttachments)
        .RestrictContentTypes(MessageContentType.PlainText),
    "premium" => new ChannelSchema(master, "Premium")
        .AddContentType(MessageContentType.Media),
    _ => master
};
```

## Health checks

### Built-in health check

```csharp
services.AddHealthChecks()
    .AddCheck<MessagingHealthCheck>("messaging", tags: ["ready"]);
```

### Manual health verification

```csharp
public async Task<bool> VerifyConnectorAsync(IChannelConnector connector)
{
    var testResult = await connector.TestConnectionAsync(CancellationToken.None);
    if (testResult.IsFailure())
        return false;

    var health = await connector.GetHealthAsync(CancellationToken.None);
    return health.IsSuccess() && health.Value?.IsHealthy == true;
}
```

### Health check pattern

```csharp
public class MessagingHealthCheck : IHealthCheck
{
    private readonly IEnumerable<IChannelConnector> _connectors;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct)
    {
        var healthy = true;
        var data = new Dictionary<string, object>();

        foreach (var connector in _connectors)
        {
            var health = await connector.GetHealthAsync(ct);
            data[$"{connector.Schema.ChannelProvider}/{connector.Schema.ChannelType}_healthy"] = health.Value?.IsHealthy;
            data[$"{connector.Schema.ChannelProvider}/{connector.Schema.ChannelType}_state"] = health.Value?.State.ToString();
            healthy &= health.Value?.IsHealthy ?? false;
        }

        return healthy
            ? HealthCheckResult.Healthy(data: data)
            : HealthCheckResult.Degraded(data: data);
    }
}
```

## Observability

### Structured logging

`ChannelConnectorBase` automatically creates structured logging scopes:

```csharp
// These scopes are active inside any connector method:
//   Connector: {Provider}/{Type}    → "Connector: Twilio/SMS"
//   Message: {message.Id}           → "Message: sms-123"

// Logging inside a connector:
Logger.LogInformation("Sending to {Receiver}", message.Receiver?.Address);

// Output with structured loggers (e.g., Serilog, Application Insights):
// [Connector: Twilio/SMS] [Message: sms-123] Sending to +15550002222
```

### Metrics to track

For production monitoring, track per-connector metrics:

| Metric | Source | What it detects |
|---|---|---|
| Send attempts | Count before `SendMessageAsync` call | Volume trends |
| Send success rate | `OperationResult.IsSuccess()` | Provider degradation |
| Send latency | Stopwatch around `SendMessageAsync` | Provider performance |
| Error codes | `OperationResult.Error.Code` | Failure type distribution |
| Connection state | `Connector.State` | Connectivity issues |

```csharp
public class MetricsDecorator : IChannelConnector
{
    private readonly IChannelConnector _inner;
    private readonly IMeterFactory _meters;

    public async Task<OperationResult<SendResult>> SendMessageAsync(
        IMessage message, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.SendMessageAsync(message, ct);
        sw.Stop();

        _meters.CreateCounter("messaging.sends").Add(1);
        _meters.CreateHistogram("messaging.latency").Record(sw.ElapsedMilliseconds);

        if (result.IsFailure())
            _meters.CreateCounter($"messaging.errors.{result.Error?.Code}").Add(1);

        return result;
    }
}
```

## Performance

### Bulk sending

Prefer `SendBatchAsync` over individual `SendMessageAsync` calls when sending multiple messages:

```csharp
var batch = new MessageBatch();
foreach (var recipient in recipients)
    batch.Messages.Add(BuildMessage(recipient));

var result = await connector.SendBatchAsync(batch, ct);
// Single HTTP request instead of N
```

### Concurrency

For high-volume sends, use bounded concurrency rather than unbounded parallelism:

```csharp
var semaphore = new SemaphoreSlim(10);
var tasks = messages.Select(async msg =>
{
    await semaphore.WaitAsync();
    try
    {
        return await connector.SendMessageAsync(msg, ct);
    }
    finally
    {
        semaphore.Release();
    }
});
var results = await Task.WhenAll(tasks);
```

### Schema caching

If you build schemas dynamically (e.g., per tenant), cache them:

```csharp
private readonly ConcurrentDictionary<string, IChannelSchema> _schemaCache = new();

public IChannelSchema GetOrBuildSchema(string tenantId)
{
    return _schemaCache.GetOrAdd(tenantId, id =>
    {
        var master = _registry.FindSchema("Twilio", "SMS");
        return new ChannelSchema(master, $"Tenant {id}")
            .UpdateParameter("WebhookUrl", p => p.DefaultValue = GetUrl(id));
    });
}
```

### Connector disposal

`ChannelConnectorBase` implements `IDisposable` and `IAsyncDisposable`. When registering via DI (`AddConnector<T>()`), the container manages disposal. For direct instantiation:

```csharp
await using var connector = new TwilioSmsConnector(schema, settings);
await connector.InitializeAsync(ct);
// use connector...
// DisposeAsync is called automatically at the end of the using block
```

## Schema versioning

As connectors evolve, their schemas change — new capabilities are added, parameters become obsolete, message property constraints are tightened. Schema versioning using semantic versioning helps track these changes and prevents incompatible schemas from being used interchangeably. The logical identity returned by `GetLogicalIdentity()` includes the version, and `IsCompatibleWith()` returns `false` for schemas with different versions.

Use semantic versioning in your schemas:

```csharp
new ChannelSchema("Twilio", "SMS", "1.0.0")  // initial release
new ChannelSchema("Twilio", "SMS", "1.1.0")  // added new capability
new ChannelSchema("Twilio", "SMS", "2.0.0")  // breaking change
```

`GetLogicalIdentity()` returns `"Twilio/SMS/1.0.0"`. Schemas with different versions are not compatible (`IsCompatibleWith` returns `false`), which prevents accidentally mixing different schema versions in runtime operations.

## Testing patterns

The layered design of the framework makes testing straightforward: you can test validation rules without a connector, test connector logic without a provider, and test provider integration with controlled sandbox environments.

### Unit testing validation rules

```csharp
[Fact]
public void AddContentType_AddsToSchema()
{
    var schema = new ChannelSchema("Test", "Unit", "1.0")
        .WithCapabilities(ChannelCapability.SendMessages)
        .AddContentType(MessageContentType.PlainText)
        .HandlesMessageEndpoint(EndpointType.Id);

    Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
}

[Fact]
public void ValidateMessage_RejectsUnsupportedContentType()
{
    var schema = new ChannelSchema("Test", "Unit", "1.0")
        .AddContentType(MessageContentType.PlainText)
        .HandlesMessageEndpoint(EndpointType.Id);

    var message = new Message()
        .WithId("test")
        .WithReceiver(Endpoint.Id("123"))
        .WithContent(new HtmlContent("<p>test</p>"));

    var issues = schema.ValidateMessage(message);
    Assert.Contains(issues, x =>
        x.ErrorMessage?.Contains("Html", StringComparison.OrdinalIgnoreCase) == true);
}
```

The schema and validation logic live entirely in memory with no I/O — test them with plain xUnit facts.

### Mocking connectors

```csharp
var mockConnector = new Mock<IChannelConnector>();

mockConnector.Setup(x => x.Schema)
    .Returns(new ChannelSchema("Test", "Mock", "1.0")
        .WithCapabilities(ChannelCapability.SendMessages));

mockConnector.Setup(x => x.SendMessageAsync(
        It.IsAny<IMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((IMessage msg, CancellationToken _) =>
        OperationResult<SendResult>.Success(new SendResult(
            msg.Id,
            "mock-remote-id")));

// Inject mockConnector.Object into your service
var service = new NotificationService(mockConnector.Object);
```

When your service depends on `IChannelConnector`, you can replace it with a mock for unit tests. This lets you test your business logic — retry policies, fallback routing, logging — without any provider dependency.

### Integration testing with real credentials

For end-to-end tests that verify the connector actually communicates with the provider, use sandbox or test credentials that do not produce real side effects:

- **Twilio**: use Test Credentials from the Twilio Console (they accept any `To` number and return mock responses)
- **SendGrid**: enable `SandboxMode` to prevent actual email delivery
- **Firebase**: enable `DryRun` to validate without sending to devices
- **Telegram**: create a test bot with BotFather
- **Facebook**: use a test page and test users from Facebook Developer Console

```csharp
var settings = new ConnectionSettings()
    .SetParameter("ProjectId", "my-project")
    .SetParameter("ServiceAccountKey", testKey)
    .SetParameter("DryRun", true);   // Firebase: validate but don't send

var connector = new FirebasePushConnector(schema, settings);
await connector.InitializeAsync(ct);
var result = await connector.SendMessageAsync(message, ct);
// result.IsSuccess == true (message was validated and accepted, but not delivered)
```

When building a custom connector, you want to test the message translation logic without calling the provider's API. A common technique is to expose the core methods through a testable subclass that bypasses initialization:

Test your connector's core logic without hitting the provider by subclassing:

```csharp
public class TestableConnector : MyConnector
{
    public TestableConnector(IChannelSchema schema)
        : base(schema, new ConnectionSettings(), NullLogger.Instance, null) { }

    public Task<SendResult> CallSendMessageCoreAsync(IMessage message, CancellationToken ct)
        => SendMessageCoreAsync(message, ct);

    public ValueTask CallInitializeAsync(CancellationToken ct)
        => InitializeConnectorAsync(ct);
}
```

```csharp
[Fact]
public async Task SendMessageCoreAsync_ReturnsSendResult()
{
    var connector = new TestableConnector(CreateSchema());
    var message = new Message().WithId("test").WithReceiver(Endpoint.Id("123"))
        .WithTextContent("Hello");

    var result = await connector.CallSendMessageCoreAsync(message, CancellationToken.None);
    Assert.NotNull(result);
    Assert.Equal("test", result.MessageId);
}
```
