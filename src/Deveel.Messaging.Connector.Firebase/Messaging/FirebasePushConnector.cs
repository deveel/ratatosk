//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Deveel.Messaging
{
    /// <summary>
    /// A channel connector that implements Firebase Cloud Messaging (FCM) push notifications.
    /// </summary>
    /// <remarks>
    /// This connector provides comprehensive support for Firebase Cloud Messaging capabilities including
    /// sending push notifications, bulk messaging, health monitoring, and various notification features
    /// such as images, actions, and platform-specific configurations.
    /// </remarks>
    [ChannelSchema(typeof(FirebasePushSchemaFactory))]
    public class FirebasePushConnector : ChannelConnectorBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly DateTime _startTime = DateTime.UtcNow;

        private string? _projectId;
        private string? _serviceAccountKey;
        private bool _dryRun;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebasePushConnector"/> class.
        /// </summary>
        /// <param name="schema">The channel schema that defines the connector's capabilities and configuration.</param>
        /// <param name="connectionSettings">The connection settings containing Firebase credentials and configuration.</param>
        /// <param name="firebaseService">The Firebase service for FCM operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <param name="authenticationManager">Optional authentication manager for handling authentication flows.</param>
        /// <exception cref="ArgumentNullException">Thrown when schema or connectionSettings is null.</exception>
        public FirebasePushConnector(IChannelSchema schema, ConnectionSettings connectionSettings, IFirebaseService? firebaseService = null, ILogger<FirebasePushConnector>? logger = null, IAuthenticationManager? authenticationManager = null)
            : base(schema, connectionSettings, logger, authenticationManager)
        {
            ArgumentNullException.ThrowIfNull(connectionSettings);
            _firebaseService = firebaseService ?? new FirebaseService();

            // Register Firebase-specific authentication provider
            AuthenticationManager.RegisterProvider(new FirebaseServiceAccountAuthenticationProvider());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebasePushConnector"/> class using the default Firebase push schema.
        /// </summary>
        /// <param name="connectionSettings">The connection settings containing Firebase credentials and configuration.</param>
        /// <param name="firebaseService">The Firebase service for FCM operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <param name="authenticationManager">Optional authentication manager for handling authentication flows.</param>
        public FirebasePushConnector(ConnectionSettings connectionSettings, IFirebaseService? firebaseService = null, ILogger<FirebasePushConnector>? logger = null, IAuthenticationManager? authenticationManager = null)
            : this(FirebaseChannelSchemas.FirebasePush, connectionSettings, firebaseService, logger, authenticationManager)
        {
        }

        /// <inheritdoc/>
        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            // Perform authentication first
            var result = await AuthenticateAsync(cancellationToken);
            if (!result.IsSuccess())
                throw new ConnectorException(
                    result.Error?.Code ?? ConnectorErrorCodes.AuthenticationFailed,
                    result.Error?.Domain ?? FirebaseErrorCodes.ErrorDomain,
                    result.Error?.Message ?? "Authentication failed");

            // Extract configuration from connection settings with proper handling of missing values
            _projectId = ConnectionSettings.GetProjectId();
            _dryRun = ConnectionSettings.GetDryRun() ?? FirebaseConnectionSettingsDefaults.DryRun;
            
            if (string.IsNullOrWhiteSpace(_projectId))
            {
                throw new MessagingException(
                    ConnectorErrorCodes.InitializationError,
                    FirebaseErrorCodes.ErrorDomain,
                    "ProjectId is required");
            }

            using var loggerScope = Logger.BeginScope("ProjectId={ProjectId}", _projectId);

            // Get the service account key from the authenticated credential
            if (AuthenticationCredential?.Scheme == AuthenticationScheme.Certificate)
            {
                _serviceAccountKey = AuthenticationCredential.Value;
            }
            else
            {
                throw new MessagingException(ConnectorErrorCodes.InitializationError,
                    FirebaseErrorCodes.ErrorDomain,
                    "Service account authentication is required for Firebase");
            }

            // Initialize Firebase service
            await _firebaseService.InitializeAsync(_serviceAccountKey, _projectId);
        }

        /// <inheritdoc/>
        protected override async ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            var isConnected = await _firebaseService.TestConnectionAsync(cancellationToken);

            if (!isConnected)
                throw new ConnectorException(ConnectorErrorCodes.ConnectionTestError,
                    FirebaseErrorCodes.ErrorDomain,
                    "Firebase connection test failed");
        }

        /// <inheritdoc/>
        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message,
            CancellationToken cancellationToken)
        {
            Logger.LogSendingPushNotification(message.Receiver?.Address);

            var firebaseMessage = await BuildFirebaseMessageAsync(message, cancellationToken);
            var messageId = await _firebaseService.SendAsync(firebaseMessage, _dryRun, cancellationToken);

            var result = new SendResult(message.Id, messageId);
            result.AdditionalData["MessageId"] = messageId;
            result.AdditionalData["ProjectId"] = _projectId!;
            result.AdditionalData["DryRun"] = _dryRun;

            return result;
        }

        /// <inheritdoc/>
        protected override async Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch,
            CancellationToken cancellationToken)
        {
            var batchId = Guid.NewGuid().ToString();
            var messages = new List<FirebaseAdmin.Messaging.Message>();
            foreach (var message in batch.Messages)
            {
                var firebaseMessage = await BuildFirebaseMessageAsync(message, cancellationToken);
                messages.Add(firebaseMessage);
            }

            var deviceTokenMessages = messages.Where(m => !string.IsNullOrEmpty(m.Token)).ToList();
            var topicMessages = messages.Where(m => !string.IsNullOrEmpty(m.Topic)).ToList();
            var results = new Dictionary<string, SendResult>();

            if (deviceTokenMessages.Count > 0)
            {
                if (deviceTokenMessages.Count > 1 && CanUseMulticast(deviceTokenMessages))
                    await SendMulticastMessagesAsync(deviceTokenMessages, batch.Messages, results, cancellationToken);
                else
                    await SendIndividualMessagesAsync(deviceTokenMessages, batch.Messages, results, cancellationToken);
            }

            if (topicMessages.Count > 0)
                await SendIndividualMessagesAsync(topicMessages, batch.Messages, results, cancellationToken);

            return new BatchSendResult(batchId, batchId, results);
        }

        /// <inheritdoc/>
        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            var status = new StatusInfo("Firebase connector operational", $"Project: {_projectId ?? "Unknown"}");
            status.AdditionalData["ProjectId"] = _projectId ?? "Unknown";
            status.AdditionalData["IsInitialized"] = _firebaseService.IsInitialized;
            status.AdditionalData["DryRun"] = _dryRun;
            status.AdditionalData["Uptime"] = DateTime.UtcNow - _startTime;
            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        protected override async Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
        {
            var health = new ConnectorHealth
            {
                State = State,
                IsHealthy = State == ConnectorState.Ready && _firebaseService.IsInitialized,
                LastHealthCheck = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - _startTime
            };

            health.Metrics["ProjectId"] = _projectId ?? "Unknown";
            health.Metrics["IsInitialized"] = _firebaseService.IsInitialized;
            health.Metrics["DryRun"] = _dryRun;

            if (!health.IsHealthy)
            {
                if (State != ConnectorState.Ready)
                    health.Issues.Add($"Connector is in {State} state");
                if (!_firebaseService.IsInitialized)
                    health.Issues.Add("Firebase service is not initialized");
                return health;
            }

            try
            {
                await TestConnectorConnectionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                health.IsHealthy = false;
                health.Issues.Add($"Connection test error: {ex.Message}");
            }

            return health;
        }

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var result in base.ValidateMessageCoreAsync(message, cancellationToken))
                yield return result;

            if (message.Receiver == null)
            {
                yield return new ValidationResult("Message receiver is required for Firebase notifications", new[] { "Receiver" });
                yield break;
            }

            if (message.Receiver.Type != EndpointType.DeviceId && message.Receiver.Type != EndpointType.Topic)
                yield return new ValidationResult("Firebase notifications only support DeviceId (device token) or Topic endpoints", new[] { "Receiver.Type" });

            if (message.Receiver.Type == EndpointType.DeviceId)
            {
                var deviceToken = message.Receiver.Address;
                if (string.IsNullOrWhiteSpace(deviceToken))
                    yield return new ValidationResult("Device token cannot be empty", new[] { "Receiver.Address" });
                else if (deviceToken.Length < 140)
                    yield return new ValidationResult("Device token appears to be invalid (too short)", new[] { "Receiver.Address" });
            }

            if (message.Receiver.Type == EndpointType.Topic)
            {
                var topicName = message.Receiver.Address;
                if (string.IsNullOrWhiteSpace(topicName))
                    yield return new ValidationResult("Topic name cannot be empty", new[] { "Receiver.Address" });
                else if (!FirebaseMessageValidator.IsValidTopicName(topicName))
                    yield return new ValidationResult("Topic name contains invalid characters. Use only letters, numbers, hyphens, and underscores", new[] { "Receiver.Address" });
            }

            if (message.Content != null)
            {
                var title = FirebaseMessageValidator.GetMessageProperty(message, "Title");
                if (!string.IsNullOrEmpty(title) && title.Length > FirebaseConnectorConstants.MaxTitleLength)
                    yield return new ValidationResult($"Notification title cannot exceed {FirebaseConnectorConstants.MaxTitleLength} characters", new[] { "Title" });

                if (message.Content is ITextContent textContent)
                {
                    if (!string.IsNullOrEmpty(textContent.Text) && textContent.Text.Length > FirebaseConnectorConstants.MaxBodyLength)
                        yield return new ValidationResult($"Message body cannot exceed {FirebaseConnectorConstants.MaxBodyLength} characters", new[] { "Content" });
                }

                var imageUrl = FirebaseMessageValidator.GetMessageProperty(message, "ImageUrl");
                if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                    yield return new ValidationResult("ImageUrl must be a valid URL", new[] { "ImageUrl" });

                var color = FirebaseMessageValidator.GetMessageProperty(message, "Color");
                if (!string.IsNullOrEmpty(color) && !FirebaseMessageValidator.IsValidHexColor(color))
                    yield return new ValidationResult("Color must be in hexadecimal format (#rrggbb or #aarrggbb)", new[] { "Color" });

                var customData = FirebaseMessageValidator.GetMessageProperty(message, "CustomData");
                if (!string.IsNullOrEmpty(customData))
                {
                    var isJson = false;
                    try { JsonDocument.Parse(customData); isJson = true; }
                    catch (JsonException) { }
                    if (!isJson)
                        yield return new ValidationResult("CustomData must be valid JSON", new[] { "CustomData" });
                }
            }
        }

        /// <summary>
        /// Builds a Firebase message from the messaging framework message.
        /// </summary>
        private Task<FirebaseAdmin.Messaging.Message> BuildFirebaseMessageAsync(IMessage message, CancellationToken cancellationToken)
            => FirebaseMessageBuilder.BuildFirebaseMessageAsync(message);

        /// <summary>
        /// Checks if messages can use multicast (same notification content).
        /// </summary>
        private bool CanUseMulticast(List<FirebaseAdmin.Messaging.Message> messages)
        {
            if (messages.Count <= 1) return false;

            var first = messages[0];
            return messages.All(m =>
                AreNotificationsEqual(m.Notification, first.Notification) &&
                AreDataPayloadsEqual(m.Data, first.Data));
        }

        /// <summary>
        /// Compares two notifications for equality.
        /// </summary>
        private bool AreNotificationsEqual(Notification? n1, Notification? n2)
        {
            if (n1 == null && n2 == null) return true;
            if (n1 == null || n2 == null) return false;

            return n1.Title == n2.Title &&
                   n1.Body == n2.Body &&
                   n1.ImageUrl == n2.ImageUrl;
        }

        /// <summary>
        /// Compares two data payloads for equality.
        /// </summary>
        private bool AreDataPayloadsEqual(IReadOnlyDictionary<string, string>? d1, IReadOnlyDictionary<string, string>? d2)
        {
            if (d1 == null && d2 == null) return true;
            if (d1 == null || d2 == null) return false;
            if (d1.Count != d2.Count) return false;

            return d1.All(kvp => d2.TryGetValue(kvp.Key, out var value) && value == kvp.Value);
        }

        /// <summary>
        /// Sends messages using multicast for efficiency.
        /// </summary>
        private async Task SendMulticastMessagesAsync(List<FirebaseAdmin.Messaging.Message> messages, IEnumerable<IMessage> originalMessages, Dictionary<string, SendResult> results, CancellationToken cancellationToken)
        {
            var tokens = messages.Select(m => m.Token!).ToList();
            var template = messages[0];

            // Split into chunks of maximum allowed tokens
            var chunks = tokens.Chunk(FirebaseConnectorConstants.MaxMulticastTokens);

            foreach (var chunk in chunks)
            {
                var multicastMessage = new MulticastMessage
                {
                    Tokens = chunk.ToList(),
                    Notification = template.Notification,
                    Data = template.Data,
                    Android = template.Android,
                    Apns = template.Apns
                };

                var batchResponse = await _firebaseService.SendMulticastAsync(multicastMessage, _dryRun, cancellationToken);

                // Process individual responses
                for (int i = 0; i < chunk.Count(); i++)
                {
                    var token = chunk.ElementAt(i);
                    var response = batchResponse.Responses[i];

                    // Find the original message ID for this token
                    var originalMessage = originalMessages.FirstOrDefault(m => m.Receiver?.Address == token);
                    var messageId = originalMessage?.Id ?? $"multicast-{token}";

                    var result = new SendResult(messageId, response.MessageId ?? "unknown");
                    result.AdditionalData["Token"] = token;
                    result.AdditionalData["ProjectId"] = _projectId!;
                    result.AdditionalData["DryRun"] = _dryRun;

                    if (response.IsSuccess)
                    {
                        result.AdditionalData["MessageId"] = response.MessageId ?? string.Empty;
                    }
                    else
                    {
                        result.AdditionalData["Error"] = response.Exception?.Message ?? "Unknown error";
                    }

                    // Use the original message ID as key
                    results[messageId] = result;
                }
            }
        }

        /// <summary>
        /// Sends messages individually.
        /// </summary>
        private async Task SendIndividualMessagesAsync(List<FirebaseAdmin.Messaging.Message> messages, IEnumerable<IMessage> originalMessages, Dictionary<string, SendResult> results, CancellationToken cancellationToken)
        {
            var batchResponse = await _firebaseService.SendEachAsync(messages, _dryRun, cancellationToken);

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                var response = batchResponse.Responses[i];

                // Find the original message ID
                var originalMessage = originalMessages.ElementAtOrDefault(i);
                var messageId = originalMessage?.Id ?? $"message-{Guid.NewGuid()}";

                var result = new SendResult(messageId, response.MessageId ?? "unknown");
                result.AdditionalData["Token"] = message.Token;
                result.AdditionalData["Topic"] = message.Topic;
                result.AdditionalData["ProjectId"] = _projectId!;
                result.AdditionalData["DryRun"] = _dryRun;

                if (response.IsSuccess)
                {
                    result.AdditionalData["MessageId"] = response.MessageId ?? string.Empty;
                }
                else
                {
                    result.AdditionalData["Error"] = response.Exception?.Message ?? "Unknown error";
                }

                // Use the original message ID as key
                results[messageId] = result;
            }
        }

        /// <inheritdoc/>
        protected override async Task ShutdownConnectorAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Clean up Firebase app if needed
                if (_firebaseService.App != null)
                {
                    _firebaseService.App.Delete();
                }
            }
            catch (Exception ex)
            {
                Logger.LogShutdownFailed(ex);
            }

            await base.ShutdownConnectorAsync(cancellationToken);
        }
    }
}
