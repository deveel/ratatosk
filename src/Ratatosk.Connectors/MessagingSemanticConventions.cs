using System.Diagnostics;

namespace Ratatosk
{
    internal static class MessagingSemanticConventions
    {
        public const string SystemName = "ratatosk";

        public const string OperationSend = "send";
        public const string OperationReceive = "receive";
        public const string OperationStatusQuery = "status_query";
        public const string OperationInitialize = "initialize";
        public const string OperationBatchSend = "batch_send";
        public const string OperationHealthCheck = "health_check";

        public const string AttributeMessagingSystem = "messaging.system";
        public const string AttributeMessagingOperation = "messaging.operation";
        public const string AttributeMessagingDestination = "messaging.destination";
        public const string AttributeMessagingMessageId = "messaging.message.id";
        public const string AttributeMessagingPayloadSize = "messaging.message.payload_size_bytes";
        public const string AttributeMessagingBatchSize = "messaging.batch.message_count";

        public const string AttributeConnectorName = "ratatosk.connector.name";
        public const string AttributeConnectorType = "ratatosk.connector.type";
        public const string AttributeConnectorState = "ratatosk.connector.state";

        public const string AttributeErrorType = "error.type";

        public const string MetricMessagesSent = "ratatosk.messages.sent";
        public const string MetricMessagesSendFailed = "ratatosk.messages.send_failed";
        public const string MetricMessagesReceived = "ratatosk.messages.received";
        public const string MetricMessagesReceiveFailed = "ratatosk.messages.receive_failed";
        public const string MetricSendDuration = "ratatosk.messages.send.duration";
        public const string MetricReceiveDuration = "ratatosk.messages.receive.duration";
        public const string MetricSendPayloadSize = "ratatosk.messages.send.payload_size";
        public const string MetricConnectorStateChanges = "ratatosk.connector.state_changes";

        public static ActivityTagsCollection CreateOperationTags(
            string connectorType,
            string operation,
            string? channelName = null,
            string? messageId = null)
        {
            var tags = new Dictionary<string, object?>
            {
                [AttributeMessagingSystem] = SystemName,
                [AttributeConnectorType] = connectorType,
                [AttributeMessagingOperation] = operation
            };

            if (channelName != null)
                tags[AttributeMessagingDestination] = channelName;

            if (messageId != null)
                tags[AttributeMessagingMessageId] = messageId;

            return new ActivityTagsCollection(tags);
        }
    }
}
