using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Ratatosk
{
    internal class ClientTelemetry : IDisposable
    {
        private const string AttrMessagingSystem = "messaging.system";
        private const string AttrMessagingOperation = "messaging.operation";
        private const string AttrMessagingMessageId = "messaging.message.id";
        private const string AttrChannelName = "messaging.destination";
        private const string AttrErrorType = "error.type";

        private static readonly string ClientSystemName = "ratatosk";

        private readonly ActivitySource _activitySource;
        private readonly Meter _meter;
        private readonly TelemetryOptions _options;

        // Send metrics
        private readonly Counter<int> _messagesSentTotal;
        private readonly Counter<int> _messagesSentFailed;
        private readonly Histogram<double> _sendDuration;

        // Receive metrics
        private readonly Counter<int> _messagesReceivedTotal;
        private readonly Counter<int> _messagesReceiveFailed;
        private readonly Histogram<double> _receiveDuration;

        // Status query metrics
        private readonly Counter<int> _statusQueriesTotal;
        private readonly Counter<int> _statusQueriesFailed;
        private readonly Histogram<double> _statusQueryDuration;

        // Receive status metrics
        private readonly Counter<int> _statusReceivesTotal;
        private readonly Counter<int> _statusReceivesFailed;
        private readonly Histogram<double> _statusReceiveDuration;

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

            _messagesReceivedTotal = _meter.CreateCounter<int>(
                "ratatosk.client.messages.received",
                unit: "{message}",
                description: "Total number of messages received through the client");

            _messagesReceiveFailed = _meter.CreateCounter<int>(
                "ratatosk.client.messages.receive_failed",
                unit: "{message}",
                description: "Total number of messages that failed to receive through the client");

            _receiveDuration = _meter.CreateHistogram<double>(
                "ratatosk.client.messages.receive.duration",
                unit: "ms",
                description: "Duration of message receive operations through the client");

            _statusQueriesTotal = _meter.CreateCounter<int>(
                "ratatosk.client.status.queries",
                unit: "{query}",
                description: "Total number of status queries through the client");

            _statusQueriesFailed = _meter.CreateCounter<int>(
                "ratatosk.client.status.query_failed",
                unit: "{query}",
                description: "Total number of status queries that failed through the client");

            _statusQueryDuration = _meter.CreateHistogram<double>(
                "ratatosk.client.status.query.duration",
                unit: "ms",
                description: "Duration of status query operations through the client");

            _statusReceivesTotal = _meter.CreateCounter<int>(
                "ratatosk.client.status.received",
                unit: "{update}",
                description: "Total number of status updates received through the client");

            _statusReceivesFailed = _meter.CreateCounter<int>(
                "ratatosk.client.status.receive_failed",
                unit: "{update}",
                description: "Total number of status updates that failed to receive through the client");

            _statusReceiveDuration = _meter.CreateHistogram<double>(
                "ratatosk.client.status.receive.duration",
                unit: "ms",
                description: "Duration of status receive operations through the client");
        }

        public Activity? StartSendActivity(string channelName, string? messageId = null)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = new ActivityTagsCollection
            {
                { AttrMessagingSystem, ClientSystemName },
                { AttrMessagingOperation, "send" },
                { AttrChannelName, channelName }
            };

            if (messageId != null)
                tags[AttrMessagingMessageId] = messageId;

            return _activitySource.StartActivity(
                $"{channelName} send",
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
                { AttrMessagingSystem, ClientSystemName },
                { AttrMessagingOperation, "receive" },
                { AttrChannelName, channelName }
            };

            return _activitySource.StartActivity(
                $"{channelName} receive",
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
                { AttrMessagingSystem, ClientSystemName },
                { AttrMessagingOperation, "status_query" },
                { AttrChannelName, channelName }
            };

            return _activitySource.StartActivity(
                $"{channelName} status_query",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        public Activity? StartReceiveStatusActivity(string channelName)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = new ActivityTagsCollection
            {
                { AttrMessagingSystem, ClientSystemName },
                { AttrMessagingOperation, "receive_status" },
                { AttrChannelName, channelName }
            };

            return _activitySource.StartActivity(
                $"{channelName} receive_status",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        // ── Send metrics ──────────────────────────────────────────────────────

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

        // ── Receive metrics ───────────────────────────────────────────────────

        public void RecordReceiveSuccess(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _messagesReceivedTotal.Add(1);
            _receiveDuration.Record(elapsedMs);
        }

        public void RecordReceiveFailure(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _messagesReceiveFailed.Add(1);
            _receiveDuration.Record(elapsedMs);
        }

        // ── Status query metrics ──────────────────────────────────────────────

        public void RecordStatusSuccess(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _statusQueriesTotal.Add(1);
            _statusQueryDuration.Record(elapsedMs);
        }

        public void RecordStatusFailure(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _statusQueriesFailed.Add(1);
            _statusQueryDuration.Record(elapsedMs);
        }

        // ── Receive status metrics ────────────────────────────────────────────

        public void RecordReceiveStatusSuccess(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _statusReceivesTotal.Add(1);
            _statusReceiveDuration.Record(elapsedMs);
        }

        public void RecordReceiveStatusFailure(long elapsedMs)
        {
            if (!_options.EnableMetrics)
                return;

            _statusReceivesFailed.Add(1);
            _statusReceiveDuration.Record(elapsedMs);
        }

        public void Dispose()
        {
            _activitySource.Dispose();
            _meter.Dispose();
        }
    }
}
