//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System.Net;
using System;
using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// A channel connector that implements email messaging using the SendGrid API.
    /// </summary>
    /// <remarks>
    /// This connector provides comprehensive support for SendGrid email capabilities including
    /// sending emails, querying message status, health monitoring, and webhook support
    /// for receiving email events and status updates.
    /// </remarks>
    [ChannelSchema(typeof(SendGridEmailSchemaFactory))]
    public class SendGridEmailConnector : ChannelConnectorBase
    {
        private readonly ISendGridService _sendGridService;
        private readonly DateTime _startTime = DateTime.UtcNow;

        private string? _apiKey;
        private bool _sandboxMode;
        private string? _webhookUrl;
        private bool _trackingSettings;
        private string? _defaultFromName;
        private string? _defaultReplyTo;
        private SendGridMessageBuilder _messageBuilder = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmailConnector"/> class.
        /// </summary>
        /// <param name="schema">The channel schema that defines the connector's capabilities and configuration.</param>
        /// <param name="connectionSettings">The connection settings containing SendGrid credentials and configuration.</param>
        /// <param name="sendGridService">The SendGrid service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when schema or connectionSettings is null.</exception>
        public SendGridEmailConnector(IChannelSchema schema, ConnectionSettings connectionSettings, ISendGridService? sendGridService = null, ILogger<SendGridEmailConnector>? logger = null)
            : base(schema, connectionSettings, logger)
        {
            ArgumentNullException.ThrowIfNull(connectionSettings);
            _sendGridService = sendGridService ?? new SendGridService();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmailConnector"/> class using one of the predefined schemas.
        /// </summary>
        /// <param name="connectionSettings">The connection settings containing SendGrid credentials and configuration.</param>
        /// <param name="sendGridService">The SendGrid service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionSettings is null.</exception>
        public SendGridEmailConnector(ConnectionSettings connectionSettings, ISendGridService? sendGridService = null, ILogger<SendGridEmailConnector>? logger = null)
            : this(SendGridChannelSchemas.SendGridEmail, connectionSettings, sendGridService, logger)
        {
        }

        /// <inheritdoc/>
        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            // Extract required parameters first
            _apiKey = ConnectionSettings.GetParameter<string>(SendGridConnectionParameters.ApiKey);

            // Extract optional parameters
            _sandboxMode = ConnectionSettings.GetParameter<bool?>(SendGridConnectionParameters.SandboxMode) ?? SendGridConnectionSettingsDefaults.SandboxMode;
            _webhookUrl = ConnectionSettings.GetParameter<string>(SendGridConnectionParameters.WebhookUrl);
            _trackingSettings = ConnectionSettings.GetParameter<bool?>(SendGridConnectionParameters.TrackingSettings) ?? SendGridConnectionSettingsDefaults.TrackingSettings;
            _defaultFromName = ConnectionSettings.GetParameter<string>(SendGridConnectionParameters.DefaultFromName);
            _defaultReplyTo = ConnectionSettings.GetParameter<string>(SendGridConnectionParameters.DefaultReplyTo);

            // Log configuration details
            Logger.LogSandboxMode(_sandboxMode);
            Logger.LogTrackingSettings(_trackingSettings);

            if (!string.IsNullOrEmpty(_webhookUrl))
                Logger.LogWebhookConfigured(_webhookUrl);

            if (!string.IsNullOrEmpty(_defaultFromName))
                Logger.LogDefaultFromName(_defaultFromName);

            if (!string.IsNullOrEmpty(_defaultReplyTo))
                Logger.LogDefaultReplyTo(_defaultReplyTo);

            // Perform custom validation logic
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new ConnectorException(SendGridErrorCodes.MissingApiKey, Schema.ChannelType, "SendGrid API Key is required");
            }

            // Initialize SendGrid client
            _sendGridService.Initialize(_apiKey);

            // Create the message builder with connector-level settings
            _messageBuilder = new SendGridMessageBuilder(_sandboxMode, _trackingSettings, _defaultReplyTo, Logger);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            // Test connection by validating API key
            var isConnected = await _sendGridService.TestConnectionAsync(cancellationToken);

            if (!isConnected)
            {
                throw new ConnectorException(SendGridErrorCodes.ConnectionFailed,
                    Schema.ChannelType,
                    "Unable to connect to SendGrid API - please verify your API key");
            }
        }

        /// <inheritdoc/>
        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message,
            CancellationToken cancellationToken)
        {
            // Note: Message validation is already performed by the base class in SendMessageAsync()
            // before calling this method, so we don't need to duplicate it here.

            // Extract and validate message properties before processing
            var messageProperties = ExtractMessageProperties(message);

            // Extract sender email
            var (senderEmail, senderName) = ExtractEmailFromEndpoint(message.Sender);
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new ConnectorException(
                    SendGridErrorCodes.MissingSender,
                    Schema.ChannelType,
                    "Sender email address is required");
            }

            if (!IsValidEmailAddress(senderEmail))
            {
                throw new ConnectorException(SendGridErrorCodes.InvalidEmailAddress,
                    Schema.ChannelType,
                    "Sender email address is not valid");
            }

            // Extract recipient email
            var (recipientEmail, recipientName) = ExtractEmailFromEndpoint(message.Receiver);
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                throw new ConnectorException(SendGridErrorCodes.InvalidRecipient,
                    Schema.ChannelType,
                    "Recipient email address is required");
            }

            if (!IsValidEmailAddress(recipientEmail))
            {
                throw new ConnectorException(SendGridErrorCodes.InvalidRecipient,
                    Schema.ChannelType,
                    "Recipient email address is not valid");
            }

            // Extract subject
            var subject = messageProperties.TryGetValue("Subject", out var subjectValue)
                ? subjectValue?.ToString()
                : "No Subject";

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ConnectorException(
                    SendGridErrorCodes.MissingEmailContent,
                    Schema.ChannelType,
                    "Email subject is required");
            }

            // Create SendGrid message
            var from = new EmailAddress(senderEmail, senderName ?? _defaultFromName);
            var to = new EmailAddress(recipientEmail, recipientName);
            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, null, null);

            // Set content based on message content type
            await _messageBuilder.SetMessageContentAsync(sendGridMessage, message);

            // Apply message settings
            _messageBuilder.ApplyMessageSettings(sendGridMessage, message, messageProperties);

            // Apply connector settings
            _messageBuilder.ApplyConnectorSettings(sendGridMessage);

            // Send the message
            var response = await _sendGridService.SendEmailAsync(sendGridMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = ExtractMessageIdFromResponse(response);

                Logger.LogInformation(
                    "Email message sent successfully. MessageId: {MessageId}, StatusCode: {StatusCode}",
                    messageId ?? message.Id, response.StatusCode);

                var result = new SendResult(message.Id, messageId ?? Guid.NewGuid().ToString())
                {
                    Status = MessageStatus.Sent,
                    Timestamp = DateTime.UtcNow
                };

                // Add properties
                result.AdditionalData["SendGridStatusCode"] = response.StatusCode.ToString();
                result.AdditionalData["To"] = recipientEmail;
                result.AdditionalData["From"] = senderEmail;
                result.AdditionalData["Subject"] = subject;
                result.AdditionalData["SandboxMode"] = _sandboxMode.ToString();

                return result;
            }
            else
            {
                var errorMessage = await response.Body.ReadAsStringAsync();
                Logger.LogError("Failed to send email. StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorMessage);

                var errorCode = response.StatusCode == HttpStatusCode.TooManyRequests
                    ? SendGridErrorCodes.RateLimitExceeded
                    : SendGridErrorCodes.SendMessageFailed;

                throw new ConnectorException(errorCode,
                    $"Failed to send email: {response.StatusCode} - {errorMessage}");
            }
        }

        /// <inheritdoc/>
        protected override async Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId,
            CancellationToken cancellationToken)
        {
            Logger.LogDebug("Querying status for message {MessageId}", messageId);

            // Note: SendGrid doesn't provide a direct message status API like Twilio
            // In a real implementation, you would need to:
            // 1. Set up Event Webhook to receive status updates
            // 2. Store status updates in your own database
            // 3. Query your database for the status

            // For this implementation, we'll simulate a basic status query
            var response = await _sendGridService.GetEmailActivityAsync(messageId, cancellationToken);

            var status = response.IsSuccessStatusCode ? MessageStatus.Delivered : MessageStatus.Unknown;
            var timestamp = DateTime.UtcNow;

            var statusUpdate = new StatusUpdateResult(messageId, status, timestamp);
            statusUpdate.AdditionalData["SendGridStatusCode"] = response.StatusCode.ToString();

            return new StatusUpdatesResult(messageId, new[] { statusUpdate });
        }

        /// <inheritdoc/>
        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            var statusInfo = new StatusInfo($"SendGrid Email Connector");

            statusInfo.AdditionalData["ApiKeyConfigured"] = !string.IsNullOrEmpty(_apiKey);
            statusInfo.AdditionalData["SandboxMode"] = _sandboxMode;
            statusInfo.AdditionalData["TrackingSettings"] = _trackingSettings;
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
                    // Test connectivity by validating API key
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

        private Dictionary<string, object?> ExtractMessageProperties(IMessage message)
        {
            var properties = new Dictionary<string, object?>();

            // Add properties from message.Properties if they exist
            if (message.Properties != null)
            {
                foreach (var property in message.Properties)
                {
                    properties[property.Key] = property.Value.Value;
                }
            }

            return properties;
        }

        private string? ExtractMessageIdFromResponse(SendGrid.Response response)
        {
            try
            {
                // SendGrid includes the message ID in the X-Message-Id header
                if (response.Headers != null)
                {
                    var messageIdHeaders = response.Headers.GetValues("X-Message-Id");
                    if (messageIdHeaders != null && messageIdHeaders.Any())
                    {
                        return messageIdHeaders.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to extract message ID from SendGrid response");
            }

            return null;
        }

        /// <summary>
        /// Validates an email address format according to basic email validation rules.
        /// </summary>
        /// <param name="emailAddress">The email address to validate.</param>
        /// <returns>True if the email address is valid, false otherwise.</returns>
        private static bool IsValidEmailAddress(string? emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return false;

            try
            {
                var emailAttribute = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
                return emailAttribute.IsValid(emailAddress);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the email address from an endpoint, handling name+email format.
        /// </summary>
        /// <param name="endpoint">The endpoint to extract email from.</param>
        /// <returns>A tuple containing the email address and optional name.</returns>
        private static (string? email, string? name) ExtractEmailFromEndpoint(IEndpoint? endpoint)
        {
            if (endpoint?.Type != EndpointType.EmailAddress || string.IsNullOrWhiteSpace(endpoint.Address))
                return (null, null);

            var address = endpoint.Address.Trim();

            // Check if it's in "Name <email@domain.com>" format
            var nameEmailPattern = @"^(.+?)\s*<(.+?)>$";
            var match = System.Text.RegularExpressions.Regex.Match(address, nameEmailPattern);

            if (match.Success)
            {
                var name = match.Groups[1].Value.Trim().Trim('"');
                var email = match.Groups[2].Value.Trim();
                return (email, name);
            }

            // Just email address
            return (address, null);
        }

        /// <inheritdoc/>
        protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source,
            CancellationToken cancellationToken)
        {
            if (source.ContentType == MessageSource.JsonContentType)
            {
                var messages = SendGridWebhookParser.ParseWebhookJson(source);

                if (messages.Count == 0)
                {
                    throw new ConnectorException(SendGridErrorCodes.InvalidWebhookData,
                        Schema.ChannelType,
                        "No valid messages found in webhook JSON");
                }

                var result = new ReceiveResult(Guid.NewGuid().ToString(), messages);
                return Task.FromResult(result);
            }

            if (source.ContentType == MessageSource.UrlPostContentType)
            {
                var formData = source.AsUrlPostData();
                var messages = SendGridWebhookParser.ParseWebhookFormData(formData);

                if (messages.Count == 0)
                {
                    throw new ConnectorException(SendGridErrorCodes.InvalidWebhookData,
                        Schema.ChannelType,
                        "No valid messages found in webhook form data");
                }

                var result = new ReceiveResult(Guid.NewGuid().ToString(), messages);
                return Task.FromResult(result);
            }

            throw new ConnectorException(SendGridErrorCodes.UnsupportedContentType,
                Schema.ChannelType,
                "Only JSON and form data are supported for SendGrid email receiving");
        }

        /// <inheritdoc/>
        protected override Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source,
            CancellationToken cancellationToken)
        {
            if (source.ContentType == MessageSource.JsonContentType)
                return Task.FromResult(SendGridWebhookParser.ParseStatusCallbackJson(source));

            if (source.ContentType == MessageSource.UrlPostContentType)
            {
                var formData = source.AsUrlPostData();
                return Task.FromResult(SendGridWebhookParser.ParseStatusCallbackFormData(formData));
            }

            throw new ConnectorException(SendGridErrorCodes.UnsupportedContentType,
                Schema.ChannelType,
                "Only JSON and form data are supported for SendGrid status callbacks");
        }

        private MessageStatus MapSendGridEventToMessageStatus(string eventType)
            => SendGridWebhookParser.MapEventToMessageStatus(eventType);
    }
}
