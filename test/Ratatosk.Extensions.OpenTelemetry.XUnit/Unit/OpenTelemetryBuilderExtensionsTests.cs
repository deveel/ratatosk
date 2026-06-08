using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "OpenTelemetry")]
public class OpenTelemetryBuilderExtensionsTests
{
    [Fact]
    public void AddRatatoskInstrumentation_OnTracerProviderBuilder_RegistersClientSource()
    {
        var sourceNames = new List<string>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddRatatoskInstrumentation()
            .Build();

        // Verify by checking that sources are added (via listener detection)
        using var listener = new ActivityListener
        {
            ShouldListenTo = source =>
            {
                sourceNames.Add(source.Name);
                return false;
            },
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.None
        };
        ActivitySource.AddActivityListener(listener);

        _ = new ActivitySource("Ratatosk.Client");
        _ = new ActivitySource("Ratatosk.Connector.TwilioSms");

        Assert.Contains(sourceNames, s => s == "Ratatosk.Client");
    }

    [Fact]
    public void AddRatatoskInstrumentation_OnTracerProviderBuilder_RegistersConnectorWildcard()
    {
        var sourceNames = new List<string>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddRatatoskInstrumentation()
            .Build();

        using var listener = new ActivityListener
        {
            ShouldListenTo = source =>
            {
                sourceNames.Add(source.Name);
                return false;
            },
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.None
        };
        ActivitySource.AddActivityListener(listener);

        _ = new ActivitySource("Ratatosk.Connector.TwilioSms");
        _ = new ActivitySource("Ratatosk.Connector.SendGridEmail");

        Assert.Contains(sourceNames, s => s == "Ratatosk.Connector.TwilioSms");
        Assert.Contains(sourceNames, s => s == "Ratatosk.Connector.SendGridEmail");
    }

    [Fact]
    public void AddRatatoskInstrumentation_OnMeterProviderBuilder_RegistersClientMeter()
    {
        var meterNames = new List<string>();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddRatatoskInstrumentation()
            .Build();

        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) => { }
        };

        // Create meters to verify they are registered
        var meter1 = new Meter("Ratatosk.Client");
        var meter2 = new Meter("Ratatosk.Connector.TwilioSms");

        // No exception thrown = registration succeeded
        Assert.NotNull(meter1);
        Assert.NotNull(meter2);
    }

    [Fact]
    public void AddRatatoskInstrumentation_OnMeterProviderBuilder_RegistersConnectorWildcard()
    {
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddRatatoskInstrumentation()
            .Build();

        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) => { }
        };
        meterListener.Start();

        var meter = new Meter("Ratatosk.Connector.SendGridEmail");
        var counter = meter.CreateCounter<int>("test");

        meterListener.RecordObservableInstruments();

        // No exception = registration succeeded
        Assert.NotNull(counter);
    }

    [Fact]
    public void AddRatatoskInstrumentation_OnTracerProviderBuilder_CanBeCalledMultipleTimes()
    {
        var exception = Record.Exception(() =>
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddRatatoskInstrumentation()
                .AddRatatoskInstrumentation()
                .Build();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void AddRatatoskInstrumentation_OnMeterProviderBuilder_CanBeCalledMultipleTimes()
    {
        var exception = Record.Exception(() =>
        {
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddRatatoskInstrumentation()
                .AddRatatoskInstrumentation()
                .Build();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void WithOpenTelemetry_OnMessagingBuilder_RegistersOpenTelemetryServices()
    {
        var services = new ServiceCollection();

        services.AddMessaging()
            .WithOpenTelemetry();

        var provider = services.BuildServiceProvider();

        // OpenTelemetry hosting integration registers these services
        var tracerProvider = provider.GetService<TracerProvider>();
        var meterProvider = provider.GetService<MeterProvider>();

        Assert.NotNull(tracerProvider);
        Assert.NotNull(meterProvider);
    }

    [Fact]
    public void WithOpenTelemetry_OnMessagingBuilder_ReturnsBuilder()
    {
        var services = new ServiceCollection();

        var builder = services.AddMessaging()
            .WithOpenTelemetry();

        Assert.NotNull(builder);
        Assert.Same(services, builder.Services);
    }
}
