//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides static methods for parsing Twilio webhook payloads and status callbacks
    /// into <see cref="IMessage"/> and <see cref="StatusUpdateResult"/> instances.
    /// This parser is shared between the SMS and WhatsApp connectors.
    /// </summary>
    internal static class TwilioMessageParser
    {
        /// <summary>
        /// Parses Twilio form-data webhook payload into a list of messages.
        /// </summary>
        /// <param name="formData">The form-data fields from the webhook POST.</param>
        /// <param name="channelType">The channel type used in error messages.</param>
        public static List<IMessage> ParseWebhookFormData(IDictionary<string, string> formData, string? channelType = null)
        {
            var messages = new List<IMessage>();

            if (!formData.TryGetValue("MessageSid", out var messageSid) || string.IsNullOrEmpty(messageSid))
                throw new ConnectorException(MessagingErrorCodes.InvalidWebhookData, TwilioErrorCodes.ErrorDomain,
                    "MessageSid is required for Twilio webhooks");

            if (!formData.TryGetValue("From", out var from) || string.IsNullOrEmpty(from))
                throw new ConnectorException(MessagingErrorCodes.InvalidWebhookData, TwilioErrorCodes.ErrorDomain,
                    "From field is required for Twilio webhooks");

            if (!formData.TryGetValue("To", out var to) || string.IsNullOrEmpty(to))
                throw new ConnectorException(MessagingErrorCodes.InvalidWebhookData, TwilioErrorCodes.ErrorDomain,
                    "To field is required for Twilio webhooks");

            var body = formData.TryGetValue("Body", out var bodyValue) ? bodyValue : "";

            var message = new Message
            {
                Id = messageSid,
                Sender = new Endpoint(GetEndpointType(from), from),
                Receiver = new Endpoint(GetEndpointType(to), to),
                Content = new TextContent(body),
                Properties = new Dictionary<string, MessageProperty>()
            };

            foreach (var kvp in formData)
            {
                if (kvp.Key != "MessageSid" && kvp.Key != "From" && kvp.Key != "To" && kvp.Key != "Body")
                    message.Properties[kvp.Key] = new MessageProperty(kvp.Key, kvp.Value);
            }

            messages.Add(message);
            return messages;
        }

        /// <summary>
        /// Parses Twilio JSON webhook payload into a list of messages.
        /// </summary>
        public static List<IMessage> ParseWebhookJson(MessageSource source)
        {
            var messages = new List<IMessage>();
            var jsonData = source.AsJson<JsonElement>();

            if (jsonData.TryGetProperty("Messages", out var messagesArray))
            {
                foreach (var messageElement in messagesArray.EnumerateArray())
                {
                    var message = ParseJsonMessage(messageElement);
                    if (message != null)
                        messages.Add(message);
                }
            }
            else
            {
                var message = ParseJsonMessage(jsonData);
                if (message != null)
                    messages.Add(message);
            }

            return messages;
        }

        /// <summary>
        /// Parses a single Twilio JSON message element into an <see cref="IMessage"/>.
        /// </summary>
        public static IMessage? ParseJsonMessage(JsonElement jsonData)
        {
            if (!jsonData.TryGetProperty("MessageSid", out var sidProperty))
                return null;

            var messageSid = sidProperty.GetString();
            if (string.IsNullOrEmpty(messageSid))
                return null;

            var from = jsonData.TryGetProperty("From", out var fromProp) ? fromProp.GetString() ?? "" : "";
            var to = jsonData.TryGetProperty("To", out var toProp) ? toProp.GetString() ?? "" : "";
            var body = jsonData.TryGetProperty("Body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return null;

            return new Message
            {
                Id = messageSid,
                Sender = new Endpoint(GetEndpointType(from), from),
                Receiver = new Endpoint(GetEndpointType(to), to),
                Content = new TextContent(body)
            };
        }

        /// <summary>
        /// Parses a Twilio status callback from form data.
        /// </summary>
        /// <param name="formData">The form-data fields from the status callback POST.</param>
        /// <param name="additionalFields">Optional extra fields to include in <see cref="StatusUpdateResult.AdditionalData"/>.</param>
        /// <param name="includeReadStatus">When <c>true</c>, maps the "read" status to <see cref="MessageStatus.Delivered"/> (WhatsApp-specific).</param>
        public static StatusUpdateResult ParseStatusCallbackFormData(
            IDictionary<string, string> formData,
            IEnumerable<string>? additionalFields = null,
            bool includeReadStatus = false)
        {
            var messageId = formData.TryGetValue("MessageSid", out var sid) ? sid : "unknown";
            var statusString = formData.TryGetValue("MessageStatus", out var status) ? status : "unknown";

            var messageStatus = MapStatusStringToMessageStatus(statusString, includeReadStatus);
            var statusResult = new StatusUpdateResult(messageId, messageStatus);

            if (formData.TryGetValue("MessagePrice", out var price))
                statusResult.AdditionalData["MessagePrice"] = price;
            if (formData.TryGetValue("MessagePriceUnit", out var priceUnit))
                statusResult.AdditionalData["MessagePriceUnit"] = priceUnit;
            if (formData.TryGetValue("ErrorCode", out var errorCode))
                statusResult.AdditionalData["ErrorCode"] = errorCode;
            if (formData.TryGetValue("ErrorMessage", out var errorMessage))
                statusResult.AdditionalData["ErrorMessage"] = errorMessage;
            if (formData.TryGetValue("To", out var to))
                statusResult.AdditionalData["To"] = to;
            if (formData.TryGetValue("From", out var from))
                statusResult.AdditionalData["From"] = from;
            if (formData.TryGetValue("AccountSid", out var accountSid))
                statusResult.AdditionalData["AccountSid"] = accountSid;

            // Include any extra fields the caller wants (e.g. WhatsApp-specific ones)
            if (additionalFields != null)
            {
                foreach (var key in additionalFields)
                {
                    if (formData.TryGetValue(key, out var value))
                        statusResult.AdditionalData[key] = value;
                }
            }

            return statusResult;
        }

        /// <summary>
        /// Parses a Twilio status callback from a JSON <see cref="MessageSource"/>.
        /// </summary>
        /// <param name="source">The message source containing the JSON payload.</param>
        /// <param name="includeReadStatus">When <c>true</c>, maps the "read" status to <see cref="MessageStatus.Delivered"/> (WhatsApp-specific).</param>
        public static StatusUpdateResult ParseStatusCallbackJson(MessageSource source, bool includeReadStatus = false)
        {
            var jsonData = source.AsJson<JsonElement>();

            var messageId = jsonData.TryGetProperty("MessageSid", out var sidProp) ? sidProp.GetString() ?? "unknown" : "unknown";
            var statusString = jsonData.TryGetProperty("MessageStatus", out var statusProp) ? statusProp.GetString() ?? "unknown" : "unknown";

            var messageStatus = MapStatusStringToMessageStatus(statusString, includeReadStatus);
            var statusResult = new StatusUpdateResult(messageId, messageStatus);

            foreach (var property in jsonData.EnumerateObject())
            {
                if (property.Name != "MessageSid" && property.Name != "MessageStatus")
                    statusResult.AdditionalData[property.Name] = property.Value.GetString() ?? "";
            }

            return statusResult;
        }

        /// <summary>
        /// Maps a Twilio status enum to a <see cref="MessageStatus"/>.
        /// </summary>
        public static MessageStatus MapStatusToMessageStatus(MessageResource.StatusEnum twilioStatus)
        {
            if (twilioStatus == MessageResource.StatusEnum.Accepted || twilioStatus == MessageResource.StatusEnum.Queued)
                return MessageStatus.Queued;
            if (twilioStatus == MessageResource.StatusEnum.Sending || twilioStatus == MessageResource.StatusEnum.Sent)
                return MessageStatus.Sent;
            if (twilioStatus == MessageResource.StatusEnum.Delivered)
                return MessageStatus.Delivered;
            if (twilioStatus == MessageResource.StatusEnum.Undelivered || twilioStatus == MessageResource.StatusEnum.Failed)
                return MessageStatus.DeliveryFailed;
            if (twilioStatus == MessageResource.StatusEnum.Received)
                return MessageStatus.Received;

            return MessageStatus.Unknown;
        }

        /// <summary>
        /// Maps a Twilio status string to a <see cref="MessageStatus"/>.
        /// </summary>
        public static MessageStatus MapStatusStringToMessageStatus(string statusString, bool includeReadStatus = false)
        {
            var status = statusString.ToLowerInvariant() switch
            {
                "delivered" => MessageStatus.Delivered,
                "sent" => MessageStatus.Sent,
                "failed" => MessageStatus.DeliveryFailed,
                "undelivered" => MessageStatus.DeliveryFailed,
                "received" => MessageStatus.Received,
                "queued" => MessageStatus.Queued,
                "accepted" => MessageStatus.Queued,
                "sending" => MessageStatus.Sent,
                _ => MessageStatus.Unknown
            };

            // WhatsApp-specific: "read" maps to Delivered
            if (status == MessageStatus.Unknown && includeReadStatus &&
                statusString.Equals("read", StringComparison.OrdinalIgnoreCase))
            {
                return MessageStatus.Delivered;
            }

            return status;
        }

        /// <summary>
        /// Determines the appropriate <see cref="EndpointType"/> for a Twilio address string.
        /// </summary>
        public static EndpointType GetEndpointType(string address)
        {
            if (string.IsNullOrEmpty(address))
                return EndpointType.Id;

            if (address.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
                return EndpointType.PhoneNumber;

            if (address.StartsWith("+"))
                return EndpointType.PhoneNumber;

            if (address.Contains('@'))
                return EndpointType.EmailAddress;

            return EndpointType.Id;
        }
    }
}




