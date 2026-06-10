---
sidebar_position: 13
---

# Telemetry (Tracing & Metrics)

Ratatosk emits OpenTelemetry-compatible tracing and metrics from every connector operation. Each connector has its own `ActivitySource` and `Meter` — traces and metrics are available out of the box without any manual instrumentation.

## Signals emitted

### Tracing (Activity)

| Operation | Span name | ActivitySource |
|---|---|---|
| `SendMessageAsync` | `{connector} send` | `Ratatosk.Connector.{type}` |
| `ReceiveMessagesAsync` | `{connector} receive` | `Ratatosk.Connector.{type}` |
| `SendBatchAsync` | `{connector} batch_send` | `Ratatosk.Connector.{type}` |
| `InitializeAsync` | `{connector} initialize` | `Ratatosk.Connector.{type}` |
| `GetStatusAsync` | `{connector} status_query` | `Ratatosk.Connector.{type}` |
| `GetMessageStatusAsync` | `{connector} status_query` | `Ratatosk.Connector.{type}` |
| `ReceiveMessageStatusAsync` | `{connector} receive_status` | `Ratatosk.Connector.{type}` |
| `GetHealthAsync` | `{connector} health_check` | `Ratatosk.Connector.{type}` |

Each span carries semantic convention attributes:

| Attribute | Example value |
|---|---|
| `messaging.system` | `ratatosk` |
| `messaging.operation` | `send`, `receive`, `status_query` |
| `messaging.destination` | Channel name |
| `messaging.message.id` | Message ID |
| `ratatosk.connector.type` | `twilio_sms` |
| `ratatosk.connector.name` | Connector instance name |
| `error.type` | Error code (on failure) |

### Metrics (Meter)

Each connector exposes a `Meter` named `Ratatosk.Connector.{type}` with the following instruments:

| Metric name | Type | Unit | Description |
|---|---|---|---|
| `ratatosk.messages.sent` | Counter | `{message}` | Total messages sent |
| `ratatosk.messages.send_failed` | Counter | `{message}` | Messages that failed to send |
| `ratatosk.messages.received` | Counter | `{message}` | Total messages received |
| `ratatosk.messages.receive_failed` | Counter | `{message}` | Messages that failed to receive |
| `ratatosk.messages.send.duration` | Histogram | `ms` | Send operation latency |
| `ratatosk.messages.receive.duration` | Histogram | `ms` | Receive operation latency |
| `ratatosk.messages.send.payload_size` | Histogram | `By` | Payload size in bytes |
| `ratatosk.connector.state_changes` | Counter | `{change}` | Connector state transitions |

The `IMessagingClient` facade also emits its own telemetry under `Ratatosk.Client`:

| Metric name | Type | Unit |
|---|---|---|
| `ratatosk.client.messages.sent` | Counter | `{message}` |
| `ratatosk.client.messages.send_failed` | Counter | `{message}` |
| `ratatosk.client.messages.send.duration` | Histogram | `ms` |

## Wiring with OpenTelemetry SDK

Add the `Ratatosk.Extensions.OpenTelemetry` package to register all sources:

```bash
dotnet add package Ratatosk.Extensions.OpenTelemetry
```

### Option 1 — Automatic (recommended)

```csharp
using Ratatosk;

builder.Services
    .AddMessaging()
    .AddTwilioSmsConnector(options => { /* ... */ })
    .WithOpenTelemetry();
```

`.WithOpenTelemetry()` internally calls `AddOpenTelemetry().WithTracing().WithMetrics()`, registers both the connector (`Ratatosk.Connector.*`) and client (`Ratatosk.Client`) sources, and manages the `TracerProvider`/`MeterProvider` lifecycle through the host.

### Option 2 — Manual on TracerProviderBuilder / MeterProviderBuilder

When you already have an OpenTelemetry setup and want to add Ratatosk sources:

```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Ratatosk;

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddRatatoskInstrumentation()
        .AddConsoleExporter()   // or any exporter
    )
    .WithMetrics(m => m
        .AddRatatoskInstrumentation()
        .AddConsoleExporter()
    );
```

`AddRatatoskInstrumentation()` adds:
- `Ratatosk.Client` — client-level ActivitySource and Meter
- `Ratatosk.Connector.*` — all connector ActivitySources and Meters (wildcard matching, new connectors are picked up automatically)

## Configuration

Telemetry can be configured per connector without modifying its provider-specific options.

### Via the connector builder

```csharp
services.AddMessaging()
    .AddConnector<TwilioSmsConnector>("sms", cfg => cfg
        .WithSettings("Twilio")
        .WithTelemetry(t =>
        {
            t.EnableTracing = true;
            t.EnableMetrics = true;
            t.EnablePayloadSizeMetrics = false;   // off by default
        }));
```

### Via ConnectionSettings

```csharp
var settings = new ConnectionSettings()
    .SetParameter("Telemetry.EnableTracing", false)
    .SetParameter("Telemetry.EnablePayloadSizeMetrics", true);

var connector = new TwilioSmsConnector(schema, settings);
```

### TelemetryOptions reference

| Property | Default | Description |
|---|---|---|
| `EnableTracing` | `true` | Emit Activity spans for connector operations |
| `EnableMetrics` | `true` | Record metric counters and histograms |
| `EnablePayloadSizeMetrics` | `false` | Serialize message to measure payload size (may impact throughput) |

### Client-level telemetry

The `IMessagingClient` facade reads its telemetry configuration from `MessagingClientOptions`:

```csharp
services.AddMessaging()
    .AddClient(options =>
    {
        options.Telemetry.EnableTracing = true;
        options.Telemetry.EnableMetrics = true;
    });
```

### ConnectionSettings keys

All telemetry settings keys are available as constants in `TelemetrySettingsKeys`:

| Key | Constant | Expected value |
|---|---|---|
| `Telemetry.EnableTracing` | `.EnableTracing` | `bool` |
| `Telemetry.EnableMetrics` | `.EnableMetrics` | `bool` |
| `Telemetry.EnablePayloadSizeMetrics` | `.EnablePayloadSizeMetrics` | `bool` |

## Performance considerations

- **Payload size metrics** require serialising the message to JSON to measure byte count — this adds CPU and memory overhead per send. Disabled by default.
- When telemetry is fully enabled, the overhead is limited to creating `Activity` objects and recording metric observations, both designed for high-throughput scenarios.
- Each connector has its own `ActivitySource` and `Meter` — if no OpenTelemetry SDK is configured (no listeners attached), `HasListeners()` returns `false` and the activity creation is skipped entirely with negligible cost.

## Viewing telemetry

Once wired, traces and metrics flow to any OpenTelemetry-compatible backend:

```bash
# Console output during development
dotnet add package OpenTelemetry.Exporter.Console

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddRatatoskInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddRatatoskInstrumentation()
        .AddConsoleExporter());
```

For production, configure an OTLP exporter:

```bash
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddRatatoskInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddRatatoskInstrumentation()
        .AddOtlpExporter());
```

## Example: tracing a send operation

```
Service A                          Twilio Connector
  │                                     │
  │ SendAsync(channel, message)          │
  │─────────────────────────────────▶    │
  │          Span: "twilio_sms send"     │
  │          tags:                       │
  │            messaging.system=ratatosk │
  │            messaging.operation=send  │
  │            messaging.message.id=...  │
  │            ratatosk.connector.type=  │
  │              twilio_sms              │
  │                                     │
  │         SendMessageCoreAsync()       │
  │         ─────────────────────────▶   │
  │         ◀────────────────────────    │
  │                                     │
  │          counter: sent +1           │
  │          histogram: send.duration   │
  │                                     │
  │◀─────────────────────────────────   │
```

## See also

- [Retry policies](retry-policies.md) — configure automatic retry and circuit breaker
- [Advanced configuration](advanced.md) — health checks, performance, testing
