using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Ratatosk
{
    internal class ClientTelemetry : IDisposable
    {
        private readonly ActivitySource _activitySource;
        private readonly Meter _meter;
        private readonly TelemetryOptions _options;

        private readonly Counter<int> _messagesSentTotal;
        private readonly Counter<int> _messagesSentFailed;
        private readonly Histogram<double> _sendDuration;

        private static readonly AssemblyVersionAttribute? _versionAttr =
            typeof(ClientTelemetry).Assembly
                .GetCustomAttributes(typeof(AssemblyVersionAttribute), false)
                .Cast<AssemblyVersionAttribute>()
                .FirstOrDefault();

        private static readonly string _version = _versionAttr?.Version ?? "0.0.0";

        public ClientTelemetry(TelemetryOptions? options = null)
        {
            _options = options ?? new TelemetryOptions();

            const string sourceName = "Ratatosk.Client";
            const string meterName = "Ratatosk.Client";

            _activitySource = new ActivitySource(sourceName, _version);
            _meter = new Meter(meterName, _version);

            _messagesSentTotal = _meter.CreateCounter<int>(
                "ratatosk.client.messages.sent",
                unit: "{message}",
                description: "Total number of messages sent through the client");

            _messagesSentFailed = _meter.CreateCounter<int>(
                "ratatosk.client.messages.send_failed",
                unit: "{message}",
                description: "Total number of messages that failed to send through the client");

            _sendDuration = _meter.CreateHistogram<double>(
                "ratatosk.client.messages.send.duration",
                unit: "ms",
                description: "Duration of message send operations through the client");
        }

        public Activity? StartSendActivity(string channelName, string? messageId = null)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = new ActivityTagsCollection
            {
                { "messaging.system", "ratatosk" },
                { "messaging.operation", "send" },
                { "ratatosk.channel.name", channelName }
            };

            if (messageId != null)
                tags["messaging.message.id"] = messageId;

            return _activitySource.StartActivity(
                $"MessagingClient send",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        public Activity? StartReceiveActivity(string channelName)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = new ActivityTagsCollection
            {
                { "messaging.system", "ratatosk" },
                { "messaging.operation", "receive" },
                { "ratatosk.channel.name", channelName }
            };

            return _activitySource.StartActivity(
                $"MessagingClient receive",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        public Activity? StartStatusActivity(string channelName)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = new ActivityTagsCollection
            {
                { "messaging.system", "ratatosk" },
                { "messaging.operation", "status_query" },
                { "ratatosk.channel.name", channelName }
            };

            return _activitySource.StartActivity(
                $"MessagingClient status_query",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        public void RecordSendSuccess(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _messagesSentTotal.Add(1);
            _sendDuration.Record(elapsedMs);
        }

        public void RecordSendFailure(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _messagesSentFailed.Add(1);
            _sendDuration.Record(elapsedMs);
        }

        public void Dispose()
        {
            _activitySource.Dispose();
            _meter.Dispose();
        }
    }
}
