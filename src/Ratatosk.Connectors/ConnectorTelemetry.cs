using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Ratatosk
{
    internal class ConnectorTelemetry : IDisposable
    {
        private readonly ActivitySource _activitySource;
        private readonly Meter _meter;
        private readonly TelemetryOptions _options;

        private readonly Counter<int> _messagesSentTotal;
        private readonly Counter<int> _messagesSentFailed;
        private readonly Counter<int> _messagesReceivedTotal;
        private readonly Counter<int> _messagesReceiveFailed;
        private readonly Histogram<double> _sendDuration;
        private readonly Histogram<double> _receiveDuration;
        private readonly Histogram<double> _sendPayloadSize;
        private readonly Counter<int> _connectorStateChanges;

        private static readonly AssemblyVersionAttribute? _versionAttr =
            typeof(ConnectorTelemetry).Assembly
                .GetCustomAttributes(typeof(AssemblyVersionAttribute), false)
                .Cast<AssemblyVersionAttribute>()
                .FirstOrDefault();

        private static readonly string _version = _versionAttr?.Version ?? "0.0.0";

        public ConnectorTelemetry(string connectorType, string? connectorInstanceName = null, TelemetryOptions? options = null)
        {
            ConnectorType = connectorType ?? throw new ArgumentNullException(nameof(connectorType));
            ConnectorInstanceName = connectorInstanceName;
            _options = options ?? new TelemetryOptions();

            var sourceName = ConnectorMeter.MakeConnectorName(connectorType);
            var meterName = ConnectorMeter.MakeConnectorName(connectorType);

            _activitySource = new ActivitySource(sourceName, _version);
            _meter = new Meter(meterName, _version);

            _messagesSentTotal = _meter.CreateCounter<int>(
                MessagingSemanticConventions.MetricMessagesSent,
                unit: "{message}",
                description: "Total number of messages sent");

            _messagesSentFailed = _meter.CreateCounter<int>(
                MessagingSemanticConventions.MetricMessagesSendFailed,
                unit: "{message}",
                description: "Total number of messages that failed to send");

            _messagesReceivedTotal = _meter.CreateCounter<int>(
                MessagingSemanticConventions.MetricMessagesReceived,
                unit: "{message}",
                description: "Total number of messages received");

            _messagesReceiveFailed = _meter.CreateCounter<int>(
                MessagingSemanticConventions.MetricMessagesReceiveFailed,
                unit: "{message}",
                description: "Total number of messages that failed to receive");

            _sendDuration = _meter.CreateHistogram<double>(
                MessagingSemanticConventions.MetricSendDuration,
                unit: "ms",
                description: "Duration of message send operations");

            _receiveDuration = _meter.CreateHistogram<double>(
                MessagingSemanticConventions.MetricReceiveDuration,
                unit: "ms",
                description: "Duration of message receive operations");

            _sendPayloadSize = _meter.CreateHistogram<double>(
                MessagingSemanticConventions.MetricSendPayloadSize,
                unit: "By",
                description: "Size of message payloads sent");

            _connectorStateChanges = _meter.CreateCounter<int>(
                MessagingSemanticConventions.MetricConnectorStateChanges,
                unit: "{change}",
                description: "Number of connector state transitions");
        }

        public string ConnectorType { get; }

        public string? ConnectorInstanceName { get; }

        public bool IsPayloadSizeEnabled => _options.EnablePayloadSizeMetrics;

        public Activity? StartActivity(string operation, string? channelName = null, string? messageId = null)
        {
            if (!_options.EnableTracing || !_activitySource.HasListeners())
                return null;

            var tags = MessagingSemanticConventions.CreateOperationTags(
                ConnectorType, operation, channelName, messageId);

            return _activitySource.StartActivity(
                $"{ConnectorInstanceName ?? ConnectorType} {operation}",
                ActivityKind.Client,
                default(ActivityContext),
                tags);
        }

        public Activity? StartSendActivity(string? channelName = null, string? messageId = null)
        {
            return StartActivity(MessagingSemanticConventions.OperationSend, channelName, messageId);
        }

        public Activity? StartReceiveActivity(string? channelName = null)
        {
            return StartActivity(MessagingSemanticConventions.OperationReceive, channelName);
        }

        public Activity? StartStatusQueryActivity(string? messageId = null)
        {
            return StartActivity(MessagingSemanticConventions.OperationStatusQuery, messageId: messageId);
        }

        public Activity? StartInitializeActivity()
        {
            return StartActivity(MessagingSemanticConventions.OperationInitialize);
        }

        public void RecordSendSuccess(long elapsedMs, int payloadSize = 0, int messageCount = 1)
        {
            if (!_options.EnableMetrics)
                return;

            var tags = CreateConnectorMetricTags();

            _messagesSentTotal.Add(messageCount, tags);

            _sendDuration.Record(elapsedMs, tags);

            if (payloadSize > 0)
                _sendPayloadSize.Record(payloadSize, tags);
        }

        public void RecordSendFailure(long elapsedMs, string? errorCode = null)
        {
            if (!_options.EnableMetrics)
                return;

            var tags = CreateConnectorMetricTags(errorCode);

            _messagesSentFailed.Add(1, tags);
            _sendDuration.Record(elapsedMs, tags);
        }

        public void RecordReceiveSuccess(long elapsedMs, int messageCount = 1)
        {
            if (!_options.EnableMetrics)
                return;

            var tags = CreateConnectorMetricTags();

            _messagesReceivedTotal.Add(messageCount, tags);
            _receiveDuration.Record(elapsedMs, tags);
        }

        public void RecordReceiveFailure(long elapsedMs, string? errorCode = null)
        {
            if (!_options.EnableMetrics)
                return;

            var tags = CreateConnectorMetricTags(errorCode);

            _messagesReceiveFailed.Add(1, tags);
            _receiveDuration.Record(elapsedMs, tags);
        }

        public void RecordStateChange(string fromState, string toState)
        {
            if (!_options.EnableMetrics)
                return;

            var tags = new TagList
            {
                { MessagingSemanticConventions.AttributeMessagingSystem, MessagingSemanticConventions.SystemName },
                { MessagingSemanticConventions.AttributeConnectorType, ConnectorType },
                { "ratatosk.connector.state.from", fromState },
                { "ratatosk.connector.state.to", toState }
            };

            _connectorStateChanges.Add(1, tags);
        }

        public long MeasurePayloadSize<T>(T message)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(message);
                return System.Text.Encoding.UTF8.GetByteCount(json);
            }
            catch
            {
                return 0;
            }
        }

        private TagList CreateConnectorMetricTags(string? errorCode = null)
        {
            var tags = new TagList
            {
                { MessagingSemanticConventions.AttributeMessagingSystem, MessagingSemanticConventions.SystemName },
                { MessagingSemanticConventions.AttributeConnectorType, ConnectorType }
            };

            if (ConnectorInstanceName != null)
                tags.Add(MessagingSemanticConventions.AttributeConnectorName, ConnectorInstanceName);

            if (errorCode != null)
                tags.Add(MessagingSemanticConventions.AttributeErrorType, errorCode);

            return tags;
        }

        public void Dispose()
        {
            _activitySource.Dispose();
            _meter.Dispose();
        }
    }
}
