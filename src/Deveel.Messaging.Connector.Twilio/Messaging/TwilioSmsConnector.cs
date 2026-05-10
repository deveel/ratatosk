//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Deveel.Messaging
{
    /// <summary>
    /// A channel connector that implements SMS messaging using the Twilio API.
    /// </summary>
    /// <remarks>
    /// This connector provides comprehensive support for Twilio SMS capabilities including
    /// sending messages, querying message status, health monitoring, and webhook support
    /// for receiving messages and status updates.
    /// </remarks>
    [ChannelSchema(typeof(TwilioSmsSchemaFactory))]
	public class TwilioSmsConnector : ChannelConnectorBase
    {
        private readonly ITwilioService _twilioService;
        private readonly DateTime _startTime = DateTime.UtcNow;

        private string? _accountSid;
        private string? _authToken;
        private string? _webhookUrl;
        private string? _statusCallback;
        private int? _validityPeriod;
        private decimal? _maxPrice;
        private string? _messagingServiceSid;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioSmsConnector"/> class.
        /// </summary>
        /// <param name="schema">The channel schema that defines the connector's capabilities and configuration.</param>
        /// <param name="connectionSettings">The connection settings containing Twilio credentials and configuration.</param>
        /// <param name="twilioService">The Twilio service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when schema or connectionSettings is null.</exception>
        public TwilioSmsConnector(IChannelSchema schema, ConnectionSettings connectionSettings, ITwilioService? twilioService = null, ILogger<TwilioSmsConnector>? logger = null)
            : base(schema, connectionSettings, logger)
        {
            ArgumentNullException.ThrowIfNull(connectionSettings, nameof(connectionSettings));
            _twilioService = twilioService ?? new TwilioService();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioSmsConnector"/> class using one of the predefined schemas.
        /// </summary>
        /// <param name="connectionSettings">The connection settings containing Twilio credentials and configuration.</param>
        /// <param name="twilioService">The Twilio service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionSettings is null.</exception>
        public TwilioSmsConnector(ConnectionSettings connectionSettings, ITwilioService? twilioService = null, ILogger<TwilioSmsConnector>? logger = null)
            : this(TwilioChannelSchemas.TwilioSms, connectionSettings, twilioService, logger)
        {
        }

        /// <inheritdoc/>
        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            // Extract required parameters first - use nullable versions to avoid exceptions
            _accountSid = ConnectionSettings.GetParameter(TwilioConnectionParameters.AccountSid) as string;
            _authToken = ConnectionSettings.GetParameter(TwilioConnectionParameters.AuthToken) as string;

            // Extract optional parameters
            _webhookUrl = ConnectionSettings.GetParameter(TwilioConnectionParameters.WebhookUrl) as string;
            _statusCallback = ConnectionSettings.GetParameter(TwilioConnectionParameters.StatusCallback) as string;
            _validityPeriod = ConnectionSettings.GetParameter(TwilioConnectionParameters.ValidityPeriod) as int?;
            _maxPrice = ConnectionSettings.GetParameter(TwilioConnectionParameters.MaxPrice) as decimal?;
            _messagingServiceSid = ConnectionSettings.GetParameter(TwilioConnectionParameters.MessagingServiceSid) as string;

            // Perform custom validation logic
            if (string.IsNullOrWhiteSpace(_accountSid) || string.IsNullOrWhiteSpace(_authToken))
            {
                throw new MessagingException(
                    TwilioErrorCodes.MissingCredentials, Schema.ChannelType,
                    "Account SID and Auth Token are required for Twilio SMS connector");
            }

            // Initialize Twilio client
            _twilioService.Initialize(_accountSid, _authToken);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            using var loggerScope = Logger.BeginScope("AccountSid = {AccountSid}", new
            {
                AccountSid = _accountSid
            });

            // Test connection by fetching account information
            var account = await _twilioService.FetchAccountAsync(_accountSid!, cancellationToken);

            if (account == null)
            {
                throw new ConnectorException(TwilioErrorCodes.ConnectionFailed,
                    "Unable to retrieve account information");
            }
        }

        /// <inheritdoc/>
        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message,
            CancellationToken cancellationToken)
        {
            // Note: Message validation is already performed by the base class in SendMessageAsync()
            // before calling this method, so we don't need to duplicate it here.

            // Extract sender phone number from message.Sender
            var senderNumber = ExtractPhoneNumber(message.Sender);
            if (string.IsNullOrWhiteSpace(senderNumber) && string.IsNullOrWhiteSpace(_messagingServiceSid))
            {
                throw new ConnectorException(TwilioErrorCodes.MissingFromNumber,
                    Schema.ChannelType,
                    "Sender phone number is required when MessagingServiceSid is not configured");
            }

            // Extract recipient phone number
            var toNumber = ExtractPhoneNumber(message.Receiver);
            if (string.IsNullOrWhiteSpace(toNumber))
            {
                throw new ConnectorException(TwilioErrorCodes.InvalidRecipient,
                    Schema.ChannelType,
                    "Recipient phone number is required and must be in E.164 format");
            }

            // Extract message body
            var messageBody = ExtractMessageBody(message);

            // Build message creation options
            var createMessageOptions = new CreateMessageOptions(new PhoneNumber(toNumber));

            // Set sender (Sender phone number or MessagingServiceSid)
            if (!string.IsNullOrWhiteSpace(_messagingServiceSid))
            {
                createMessageOptions.MessagingServiceSid = _messagingServiceSid;
            }
            else if (!string.IsNullOrWhiteSpace(senderNumber))
            {
                createMessageOptions.From = new PhoneNumber(senderNumber);
            }

            // Set message content
            if (!string.IsNullOrWhiteSpace(messageBody))
            {
                createMessageOptions.Body = messageBody;
            }

            // Add media URLs if present
            var mediaUrls = ExtractMediaUrls(message);
            if (mediaUrls?.Count > 0)
            {
                createMessageOptions.MediaUrl = mediaUrls;
            }

            // Apply message-specific or connector-level settings
            ApplyMessageSettings(createMessageOptions, message);

            // Send the message
            var messageResource = await _twilioService.CreateMessageAsync(createMessageOptions, cancellationToken);

            Logger?.LogInformation("SMS message sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);

            var result = new SendResult(message.Id, messageResource.Sid)
            {
                Status = MapTwilioStatusToMessageStatus(messageResource.Status),
                Timestamp = messageResource.DateCreated ?? DateTime.UtcNow
            };

            // Add properties
            result.AdditionalData["TwilioSid"] = messageResource.Sid;
            result.AdditionalData["TwilioStatus"] = messageResource.Status.ToString();
            result.AdditionalData["To"] = messageResource.To;
            result.AdditionalData["From"] = messageResource.From ?? senderNumber ?? "";
            result.AdditionalData["NumSegments"] = messageResource.NumSegments ?? "0";

            if (!string.IsNullOrWhiteSpace(messageResource.Price))
            {
                result.AdditionalData["Price"] = messageResource.Price;
                result.AdditionalData["PriceUnit"] = messageResource.PriceUnit ?? "USD";
            }

            return result;
        }

        /// <inheritdoc/>
        protected override async Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId,
            CancellationToken cancellationToken)
        {
            Logger.LogDebug("Querying status for message {MessageId}", messageId);

            // Assume messageId is the Twilio SID
            var messageResource = await _twilioService.FetchMessageAsync(messageId, cancellationToken);
            var timestamp = messageResource.DateUpdated ?? messageResource.DateCreated ?? DateTime.UtcNow;
            var status = MapTwilioStatusToMessageStatus(messageResource.Status);

            var statusUpdate = new StatusUpdateResult(messageId, status, timestamp);

            statusUpdate.AdditionalData["TwilioStatus"] = messageResource.Status.ToString();
            statusUpdate.AdditionalData["ErrorCode"] = messageResource.ErrorCode ?? 0;
            statusUpdate.AdditionalData["ErrorMessage"] = messageResource.ErrorMessage ?? "";
            statusUpdate.AdditionalData["NumSegments"] = messageResource.NumSegments ?? "0";

            if (!string.IsNullOrWhiteSpace(messageResource.Price))
            {
                statusUpdate.AdditionalData["Price"] = messageResource.Price;
                statusUpdate.AdditionalData["PriceUnit"] = messageResource.PriceUnit ?? "USD";
            }

            return new StatusUpdatesResult(messageId, new[] { statusUpdate });
        }

        /// <inheritdoc/>
        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            var statusInfo = new StatusInfo($"Twilio SMS Connector (Account: {_accountSid})");

            statusInfo.AdditionalData["AccountSid"] = _accountSid ?? "";
            statusInfo.AdditionalData["MessagingServiceSid"] = _messagingServiceSid ?? "";
            statusInfo.AdditionalData["State"] = State.ToString();
            statusInfo.AdditionalData["Uptime"] = DateTime.UtcNow - _startTime;

            return Task.FromResult(statusInfo);
        }

        /// <inheritdoc/>
        protected override async Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
        {
            var health = new ConnectorHealth
            {
                State = State,
                IsHealthy = State == ConnectorState.Ready,
                LastHealthCheck = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - _startTime
            };

            if (State == ConnectorState.Ready)
            {
                try
                {
                    // Test connectivity by fetching account info
                    await TestConnectorConnectionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    health.IsHealthy = false;
                    health.Issues.Add($"Health check failed: {ex.Message}");
                }
            }
            else
            {
                health.Issues.Add($"Connector is in {State} state");
            }

            return health;
        }

        private string? ExtractPhoneNumber(IEndpoint? endpoint)
        {
            if (endpoint?.Type == EndpointType.PhoneNumber)
            {
                return endpoint.Address;
            }
            return null;
        }

        private string? ExtractMessageBody(IMessage message)
        {
            if (message.Content?.ContentType == MessageContentType.PlainText && message.Content is ITextContent textContent)
            {
                return textContent.Text;
            }
            return null;
        }

        private List<Uri>? ExtractMediaUrls(IMessage message)
        {
            if (message.Content?.ContentType == MessageContentType.Media && message.Content is IMediaContent mediaContent)
            {
                if (!string.IsNullOrWhiteSpace(mediaContent.FileUrl))
                {
                    try
                    {
                        return new List<Uri> { new Uri(mediaContent.FileUrl) };
                    }
                    catch (UriFormatException ex)
                    {
                        Logger?.LogWarning(ex, "Invalid media URL format: {MediaUrl}", mediaContent.FileUrl);
                        return null;
                    }
                }
            }
            return null;
        }

        private void ApplyMessageSettings(CreateMessageOptions options, IMessage message)
        {
            // Apply validity period
            if (_validityPeriod.HasValue)
            {
                options.ValidityPeriod = _validityPeriod.Value;
            }

            // Apply max price
            if (_maxPrice.HasValue)
            {
                options.MaxPrice = _maxPrice.Value;
            }

            // Apply status callback
            if (!string.IsNullOrWhiteSpace(_statusCallback))
            {
                options.StatusCallback = new Uri(_statusCallback);
            }

            // Apply message-specific properties if available
            if (message.Properties != null)
            {
                foreach (var property in message.Properties)
                {
                    switch (property.Key.ToLowerInvariant())
                    {
                        case "validityperiod":
                            if (int.TryParse(property.Value?.Value?.ToString(), out var validityPeriod))
                            {
                                options.ValidityPeriod = validityPeriod;
                            }
                            break;
                        case "maxprice":
                            if (decimal.TryParse(property.Value?.Value?.ToString(), out var maxPrice))
                            {
                                options.MaxPrice = maxPrice;
                            }
                            break;
                        case "providecallback":
                            if (bool.TryParse(property.Value?.Value?.ToString(), out var provideCallback) &&
                                provideCallback && !string.IsNullOrWhiteSpace(_statusCallback))
                            {
                                options.StatusCallback = new Uri(_statusCallback);
                            }
                            break;
                    }
                }
            }
        }

        private MessageStatus MapTwilioStatusToMessageStatus(MessageResource.StatusEnum twilioStatus)
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

        /// <inheritdoc/>
        protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source,
            CancellationToken cancellationToken)
        {
            if (source.ContentType == MessageSource.UrlPostContentType)
            {
                var formData = source.AsUrlPostData();
                var messages = ParseTwilioWebhookFormData(formData);

                if (messages.Count == 0)
                {
                    throw new ConnectorException(TwilioErrorCodes.InvalidWebhookData,
                        "No valid messages found in webhook form data");
                }

                return Task.FromResult(new ReceiveResult(Guid.NewGuid().ToString(), messages));
            }

            if (source.ContentType == MessageSource.JsonContentType)
            {
                var messages = ParseTwilioWebhookJson(source);

                if (messages.Count == 0)
                {
                    throw new ConnectorException(TwilioErrorCodes.InvalidWebhookData,
                        "No valid messages found in webhook JSON");
                }

                var result = new ReceiveResult(Guid.NewGuid().ToString(), messages);
                return Task.FromResult(result);
            }

            throw new ConnectorException(TwilioErrorCodes.UnsupportedContentType,
                "Only form data and JSON are supported for Twilio message receiving");
        }

        /// <inheritdoc/>
        protected override Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source,
            CancellationToken cancellationToken)
        {
            if (source.ContentType == MessageSource.UrlPostContentType)
            {
                var formData = source.AsUrlPostData();
                var statusResult = ParseTwilioStatusCallbackFormData(formData);

                return Task.FromResult(statusResult);
            }

            if (source.ContentType == MessageSource.JsonContentType)
            {
                var statusResult = ParseTwilioStatusCallbackJson(source);
                return Task.FromResult(statusResult);
            }

            throw new ConnectorException(TwilioErrorCodes.UnsupportedContentType,
                "Only form data and JSON are supported for Twilio status callbacks");
        }

        private List<IMessage> ParseTwilioWebhookFormData(IDictionary<string, string> formData)
        {
            var messages = new List<IMessage>();

            // Validate required fields for Twilio SMS webhook
            if (!formData.TryGetValue("MessageSid", out var messageSid) || string.IsNullOrEmpty(messageSid))
            {
                throw new ArgumentException("MessageSid is required for Twilio SMS webhooks");
            }

            if (!formData.TryGetValue("From", out var from) || string.IsNullOrEmpty(from))
            {
                throw new ArgumentException("From field is required for Twilio SMS webhooks");
            }

            if (!formData.TryGetValue("To", out var to) || string.IsNullOrEmpty(to))
            {
                throw new ArgumentException("To field is required for Twilio SMS webhooks");
            }

            // Body is optional for MMS messages
            var body = formData.TryGetValue("Body", out var bodyValue) ? bodyValue : "";

            // Create the message
            var message = new Message
            {
                Id = messageSid,
                Sender = new Endpoint(GetTwilioEndpointType(from), from),
                Receiver = new Endpoint(GetTwilioEndpointType(to), to),
                Content = new TextContent(body),
                Properties = new Dictionary<string, MessageProperty>()
            };

            // Add all other Twilio webhook fields as message properties
            foreach (var kvp in formData)
            {
                if (kvp.Key != "MessageSid" && kvp.Key != "From" && kvp.Key != "To" && kvp.Key != "Body")
                {
                    message.Properties[kvp.Key] = new MessageProperty(kvp.Key, kvp.Value);
                }
            }

            messages.Add(message);
            return messages;
        }

        private List<IMessage> ParseTwilioWebhookJson(MessageSource source)
        {
            var messages = new List<IMessage>();
            var jsonData = source.AsJson<System.Text.Json.JsonElement>();

            if (jsonData.TryGetProperty("Messages", out var messagesArray))
            {
                // Batch messages
                foreach (var messageElement in messagesArray.EnumerateArray())
                {
                    var message = ParseTwilioJsonMessage(messageElement);
                    if (message != null)
                        messages.Add(message);
                }
            }
            else
            {
                // Single message
                var message = ParseTwilioJsonMessage(jsonData);
                if (message != null)
                    messages.Add(message);
            }

            return messages;
        }

        private IMessage? ParseTwilioJsonMessage(System.Text.Json.JsonElement jsonData)
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
                Sender = new Endpoint(GetTwilioEndpointType(from), from),
                Receiver = new Endpoint(GetTwilioEndpointType(to), to),
                Content = new TextContent(body)
            };
        }

        private StatusUpdateResult ParseTwilioStatusCallbackFormData(IDictionary<string, string> formData)
        {
            var messageId = formData.TryGetValue("MessageSid", out var sid) ? sid : "unknown";
            var statusString = formData.TryGetValue("MessageStatus", out var status) ? status : "unknown";

            var messageStatus = MapTwilioStatusStringToMessageStatus(statusString);
            var statusResult = new StatusUpdateResult(messageId, messageStatus);

            // Add additional Twilio status data
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

            return statusResult;
        }

        private StatusUpdateResult ParseTwilioStatusCallbackJson(MessageSource source)
        {
            var jsonData = source.AsJson<System.Text.Json.JsonElement>();

            var messageId = jsonData.TryGetProperty("MessageSid", out var sidProp) ? sidProp.GetString() ?? "unknown" : "unknown";
            var statusString = jsonData.TryGetProperty("MessageStatus", out var statusProp) ? statusProp.GetString() ?? "unknown" : "unknown";

            var messageStatus = MapTwilioStatusStringToMessageStatus(statusString);
            var statusResult = new StatusUpdateResult(messageId, messageStatus);

            // Add additional JSON properties as additional data
            foreach (var property in jsonData.EnumerateObject())
            {
                if (property.Name != "MessageSid" && property.Name != "MessageStatus")
                {
                    statusResult.AdditionalData[property.Name] = property.Value.GetString() ?? "";
                }
            }

            return statusResult;
        }

        private MessageStatus MapTwilioStatusStringToMessageStatus(string statusString)
        {
            return statusString.ToLowerInvariant() switch
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
        }

        private static EndpointType GetTwilioEndpointType(string address)
        {
            if (string.IsNullOrEmpty(address))
                return EndpointType.Id;

            if (address.StartsWith("whatsapp:"))
                return EndpointType.PhoneNumber;

            if (address.StartsWith("+"))
                return EndpointType.PhoneNumber;

            if (address.Contains("@"))
                return EndpointType.EmailAddress;

            return EndpointType.Id;
        }
    }
}
