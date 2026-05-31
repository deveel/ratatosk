//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

namespace Ratatosk
{
    /// <summary>
    /// Provides static methods for parsing SendGrid webhook payloads and status callbacks
    /// into <see cref="IMessage"/> and <see cref="StatusUpdateResult"/> instances.
    /// </summary>
    internal static class SendGridWebhookParser
    {
        /// <summary>
        /// Parses a SendGrid JSON webhook payload into a list of messages.
        /// </summary>
        public static List<IMessage> ParseWebhookJson(MessageSource source)
        {
            var messages = new List<IMessage>();
            var jsonData = source.AsJson<JsonElement>();

            if (jsonData.ValueKind == JsonValueKind.Array)
            {
                foreach (var eventElement in jsonData.EnumerateArray())
                {
                    var message = ParseJsonEvent(eventElement);
                    if (message != null)
                        messages.Add(message);
                }
            }
            else
            {
                var message = ParseJsonEvent(jsonData);
                if (message != null)
                    messages.Add(message);
            }

            return messages;
        }

        /// <summary>
        /// Parses a single SendGrid webhook event JSON element into an <see cref="IMessage"/>.
        /// Returns <c>null</c> if the event is not an inbound/processed email.
        /// </summary>
        public static IMessage? ParseJsonEvent(JsonElement eventData)
        {
            if (!eventData.TryGetProperty("event", out var eventProperty))
                return null;

            var eventType = eventProperty.GetString();
            if (eventType != "inbound" && eventType != "processed")
                return null;

            var messageId = eventData.TryGetProperty("sg_message_id", out var idProp)
                ? idProp.GetString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            var from = eventData.TryGetProperty("from", out var fromProp) ? fromProp.GetString() ?? "" : "";
            var to = eventData.TryGetProperty("to", out var toProp) ? toProp.GetString() ?? "" : "";

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return null;

            var subject = eventData.TryGetProperty("subject", out var subjectProp) ? subjectProp.GetString() ?? "" : "";
            var text = eventData.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? "" : "";
            var html = eventData.TryGetProperty("html", out var htmlProp) ? htmlProp.GetString() ?? "" : "";

            MessageContent content = !string.IsNullOrEmpty(html)
                ? new HtmlContent(html)
                : new TextContent(text);

            var message = new Message
            {
                Id = messageId,
                Sender = new Endpoint(EndpointType.EmailAddress, from),
                Receiver = new Endpoint(EndpointType.EmailAddress, to),
                Content = content,
                Properties = new Dictionary<string, MessageProperty>
                {
                    ["Subject"] = new MessageProperty("Subject", subject)
                }
            };

            foreach (var property in eventData.EnumerateObject())
            {
                if (property.Name != "sg_message_id" && property.Name != "from" &&
                    property.Name != "to" && property.Name != "subject" &&
                    property.Name != "text" && property.Name != "html")
                {
                    var value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? "",
                        JsonValueKind.Number => property.Value.GetInt64().ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Array => property.Value.ToString(),
                        JsonValueKind.Object => property.Value.ToString(),
                        _ => property.Value.ToString()
                    };
                    message.Properties[property.Name] = new MessageProperty(property.Name, value);
                }
            }

            return message;
        }

        /// <summary>
        /// Parses a SendGrid inbound parse webhook from form data.
        /// </summary>
        public static List<IMessage> ParseWebhookFormData(IDictionary<string, string> formData)
        {
            var messages = new List<IMessage>();

            if (!formData.TryGetValue("from", out var from) || string.IsNullOrEmpty(from))
                throw new ArgumentException("from field is required for SendGrid webhooks");

            if (!formData.TryGetValue("to", out var to) || string.IsNullOrEmpty(to))
                throw new ArgumentException("to field is required for SendGrid webhooks");

            var messageId = formData.TryGetValue("envelope", out var envelope)
                ? envelope : Guid.NewGuid().ToString();

            var subject = formData.TryGetValue("subject", out var subjectValue) ? subjectValue : "";
            var text = formData.TryGetValue("text", out var textValue) ? textValue : "";
            var html = formData.TryGetValue("html", out var htmlValue) ? htmlValue : "";

            MessageContent content = !string.IsNullOrEmpty(html)
                ? new HtmlContent(html)
                : new TextContent(text);

            var message = new Message
            {
                Id = messageId,
                Sender = new Endpoint(EndpointType.EmailAddress, from),
                Receiver = new Endpoint(EndpointType.EmailAddress, to),
                Content = content,
                Properties = new Dictionary<string, MessageProperty>
                {
                    ["Subject"] = new MessageProperty("Subject", subject)
                }
            };

            foreach (var kvp in formData)
            {
                if (kvp.Key != "from" && kvp.Key != "to" && kvp.Key != "subject" &&
                    kvp.Key != "text" && kvp.Key != "html" && kvp.Key != "envelope")
                {
                    message.Properties[kvp.Key] = new MessageProperty(kvp.Key, kvp.Value);
                }
            }

            messages.Add(message);
            return messages;
        }

        /// <summary>
        /// Parses a SendGrid status/event callback from a JSON <see cref="MessageSource"/>.
        /// </summary>
        public static StatusUpdateResult ParseStatusCallbackJson(MessageSource source)
        {
            var jsonData = source.AsJson<JsonElement>();

            if (jsonData.ValueKind == JsonValueKind.Array)
            {
                if (jsonData.GetArrayLength() > 0)
                    jsonData = jsonData.EnumerateArray().First();
                else
                    throw new ArgumentException("Empty events array in SendGrid webhook");
            }

            var messageId = jsonData.TryGetProperty("sg_message_id", out var sidProp)
                ? sidProp.GetString() ?? "unknown" : "unknown";
            var eventType = jsonData.TryGetProperty("event", out var eventProp)
                ? eventProp.GetString() ?? "unknown" : "unknown";

            var messageStatus = MapEventToMessageStatus(eventType);
            var timestamp = DateTime.UtcNow;

            if (jsonData.TryGetProperty("timestamp", out var timestampProp) &&
                timestampProp.ValueKind == JsonValueKind.Number &&
                timestampProp.TryGetInt64(out var unixTimestamp))
            {
                timestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
            }

            var statusResult = new StatusUpdateResult(messageId, messageStatus, timestamp);

            foreach (var property in jsonData.EnumerateObject())
            {
                if (property.Name != "sg_message_id")
                {
                    var value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? "",
                        JsonValueKind.Number => property.Value.GetInt64().ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Array => property.Value.ToString(),
                        JsonValueKind.Object => property.Value.ToString(),
                        _ => property.Value.ToString()
                    };
                    statusResult.AdditionalData[property.Name] = value;
                }
            }

            statusResult.AdditionalData["Channel"] = "Email";
            statusResult.AdditionalData["Provider"] = "SendGrid";

            return statusResult;
        }

        /// <summary>
        /// Parses a SendGrid status/event callback from form data.
        /// </summary>
        public static StatusUpdateResult ParseStatusCallbackFormData(IDictionary<string, string> formData)
        {
            var messageId = formData.TryGetValue("sg_message_id", out var sid) ? sid : "unknown";
            var eventType = formData.TryGetValue("event", out var evt) ? evt : "unknown";

            var messageStatus = MapEventToMessageStatus(eventType);
            var timestamp = DateTime.UtcNow;

            if (formData.TryGetValue("timestamp", out var timestampString) &&
                long.TryParse(timestampString, out var unixTimestamp))
            {
                timestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
            }

            var statusResult = new StatusUpdateResult(messageId, messageStatus, timestamp);

            foreach (var kvp in formData)
            {
                if (kvp.Key != "sg_message_id")
                    statusResult.AdditionalData[kvp.Key] = kvp.Value;
            }

            statusResult.AdditionalData["Channel"] = "Email";
            statusResult.AdditionalData["Provider"] = "SendGrid";

            return statusResult;
        }

        /// <summary>
        /// Maps a SendGrid event type string to a <see cref="MessageStatus"/>.
        /// </summary>
        public static MessageStatus MapEventToMessageStatus(string eventType)
        {
            return eventType.ToLowerInvariant() switch
            {
                "processed" => MessageStatus.Queued,
                "deferred" => MessageStatus.Queued,
                "delivered" => MessageStatus.Delivered,
                "open" => MessageStatus.Delivered,
                "click" => MessageStatus.Delivered,
                "bounce" => MessageStatus.DeliveryFailed,
                "dropped" => MessageStatus.DeliveryFailed,
                "spamreport" => MessageStatus.DeliveryFailed,
                "unsubscribe" => MessageStatus.Delivered,
                "group_unsubscribe" => MessageStatus.Delivered,
                "group_resubscribe" => MessageStatus.Delivered,
                "inbound" => MessageStatus.Received,
                _ => MessageStatus.Unknown
            };
        }
    }
}

