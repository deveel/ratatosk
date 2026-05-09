# Advanced Configuration

This page covers common production patterns without locking you into one architecture.

## Environment-specific setup

Use one base schema per connector family, then derive environment-specific or tenant-specific restrictions.

```csharp
var productionSchema = new ChannelSchema(baseSchema, "Production")
    .UpdateParameter("Timeout", p => p.DefaultValue = 30000);
```

## Connector decorators

Decorators work well for cross-cutting concerns:

- audit logging
- metrics/tracing
- retry and circuit breaker policies
- batching/bulk adaptation

Keep provider connectors simple and move operational behavior to decorators.

## Security basics

- Store secrets in vault/env vars, not in source
- Mark schema parameters as `IsSensitive`
- Validate webhook signatures for inbound channels
- Redact sensitive values in logs

## Performance basics

- Prefer provider bulk APIs when available
- Use bounded concurrency for high-volume sends
- Cache static schema artifacts if you build them dynamically
- Apply backoff for transient provider errors

## Health and observability

Add health checks that call `TestConnectionAsync` where supported.

Track at least:

- send attempts/success/failures
- provider latency
- error codes grouped by connector

## Related docs

- [Connector implementation](channelconnector-usage.md)
- [Channel registry guide](channelregistry-guide.md)
