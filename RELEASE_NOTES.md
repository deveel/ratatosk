# v1.1.2 — OpenTelemetry

**Release date:** 2026-06-09

## Summary

OpenTelemetry instrumentation is now a first-class citizen in Ratatosk. This release introduces a new package `Ratatosk.Extensions.OpenTelemetry` along with comprehensive tracing and metrics support across the client facade, the connectors layer, and each individual channel connector.

## New Package

- **`Ratatosk.Extensions.OpenTelemetry`** — Convenience extensions for wiring up Ratatosk's telemetry sources into OpenTelemetry. Provides `AddRatatoskInstrumentation()` methods on `TracerProviderBuilder` and `MeterProviderBuilder`, and a `WithOpenTelemetry()` shortcut on `MessagingBuilder`.

## Tracing (ActivitySources)

- `Ratatosk.Client` — spans from the `IMessagingClient` facade covering send, receive, and status operations.
- `Ratatosk.Connector.*` — per-connector spans (Twilio, SendGrid, Firebase, Facebook, Telegram) for fine-grained observability.

## Metrics (Meters)

- `Ratatosk.Client` — client-level counters and histograms (sent count, send duration, etc.).
- `Ratatosk.Connector.*` — per-connector metrics (sent/received/failed counts, latency histograms).
- New `ConnectorMeter` utility class for consistent meter naming across connectors.

## New Request Classes

- `SendRequest`, `ReceiveRequest`, `ReceiveStatusRequest`, `StatusRequest`, `BatchSendRequest` — strongly-typed request objects that carry operation parameters and telemetry context, enabling richer activity tracking.

## Telemetry Infrastructure

- `ClientTelemetry` — centralized telemetry coordination for the messaging client.
- `ConnectorTelemetry` — per-connector telemetry implementation handling activities and metrics.
- `MessagingSemanticConventions` — semantic convention constants for messaging attributes (messaging system, destination name, operation type, etc.).
- `MessageContext` — context object for attaching trace information to message delivery.

## Other

- `ChannelConnectorBase` refactored to integrate with new telemetry infrastructure.
- `MessagingClient` significantly refactored to leverage request classes and centralized telemetry.
- New test projects: OpenTelemetry extension tests, connector telemetry tests.
- New documentation: `docs/telemetry.md` — comprehensive telemetry guide.
- Updated `ROADMAP.md` and `README.md`.
- Solution file updated to include the new package.

## Full Changelog

```
eb200be feat: add OpenTelemetry support for tracing and metrics in Ratatosk framework
ef97672 feat: add ConnectorMeter utility for consistent meter naming in telemetry
88b3c91 feat: enhance telemetry metrics with message count and improve activity scope management
7f1b2e4 feat: enhance telemetry with additional messaging attributes and improve activity tracking
9a39bf6 feat: enhance telemetry metrics for message sending, receiving, and status queries
4b9af24 feat: introduce request classes for messaging operations and enhance telemetry context handling
```

**Files changed:** 36 files, +3,089 / −681
