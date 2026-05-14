//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// A channel connector that implements messaging using the Facebook Messenger Platform API.
    /// </summary>
    /// <remarks>
    /// This connector provides comprehensive support for Facebook Messenger capabilities including
    /// sending messages, receiving messages via webhooks, media attachments, and health monitoring.
    /// </remarks>
    [ChannelSchema(typeof(FacebookMessengerSchemaFactory))]
    public class FacebookMessengerConnector : ChannelConnectorBase
    {
        private readonly IFacebookService _facebookService;
        private readonly DateTime _startTime = DateTime.UtcNow;

        private string? _pageAccessToken;
        private string? _pageId;
        private string? _webhookUrl;
        private string? _verifyToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookMessengerConnector"/> class.
        /// </summary>
        /// <param name="schema">The channel schema that defines the connector's capabilities and configuration.</param>
        /// <param name="connectionSettings">The connection settings containing Facebook credentials and configuration.</param>
        /// <param name="facebookService">The Facebook service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when schema or connectionSettings is null.</exception>
        public FacebookMessengerConnector(IChannelSchema schema, ConnectionSettings connectionSettings, IFacebookService? facebookService = null, ILogger<FacebookMessengerConnector>? logger = null)
            : base(schema, connectionSettings, logger)
        {
            _facebookService = facebookService ?? new FacebookService();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookMessengerConnector"/> class using one of the predefined schemas.
        /// </summary>
        /// <param name="connectionSettings">The connection settings containing Facebook credentials and configuration.</param>
        /// <param name="facebookService">The Facebook service for API operations.</param>
        /// <param name="logger">Optional logger for diagnostic and operational logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionSettings is null.</exception>
        public FacebookMessengerConnector(ConnectionSettings connectionSettings, IFacebookService? facebookService = null, ILogger<FacebookMessengerConnector>? logger = null)
            : this(FacebookChannelSchemas.FacebookMessenger, connectionSettings, facebookService, logger)
        {
        }

        /// <inheritdoc/>
        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            _pageId = ConnectionSettings.GetParameter<string?>(FacebookConnectionParameters.PageId);
            _webhookUrl = ConnectionSettings.GetParameter<string?>(FacebookConnectionParameters.WebhookUrl);
            _verifyToken = ConnectionSettings.GetParameter<string?>(FacebookConnectionParameters.VerifyToken);

            if (string.IsNullOrWhiteSpace(_pageId))
            {
                throw new MessagingException(
                    FacebookErrorCodes.MissingPageId, Schema.ChannelType,
                    "Page ID is required");
            }

            _pageAccessToken = AuthenticationCredential?.Value;
            if (string.IsNullOrWhiteSpace(_pageAccessToken))
            {
                throw new MessagingException(
                    FacebookErrorCodes.MissingCredentials, Schema.ChannelType,
                    "Page Access Token is required");
            }

            try
            {
                _facebookService.Initialize(_pageAccessToken);
            }
            catch (ArgumentException ex)
            {
                throw new ConnectorException(
                    FacebookErrorCodes.InvalidAccessToken, Schema.ChannelType,
                    ex.Message, ex);
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            using var scope = Logger.BeginScope("PageID={PageId}", new
            {
                PageId = _pageId
            });

            try
            {
                // Test connection by fetching page information using Graph API
                var page = await _facebookService.FetchPageAsync(_pageId!, cancellationToken);

                if (page == null)
                {
                    throw new ConnectorException(
                        FacebookErrorCodes.MissingPageId,
                        Schema.ChannelType,
                        "Unable to retrieve page information - page may not exist or access token may be invalid");
                }

                Logger?.LogConnectionTestSuccessful(page.Name, page.Category);
            } catch (InvalidOperationException ex) when (ex.Message.Contains("Facebook Graph API error"))
            {
                Logger?.LogConnectionTestGraphApiError(ex.Message, ex);
                throw new ConnectorException(FacebookErrorCodes.ConnectionTestFailed,
                    Schema.ChannelType,
                    $"Facebook Graph API error during connection test: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message,
            CancellationToken cancellationToken)
        {
            // Extract recipient User ID first to get the right error code
            var recipientId = FacebookMessageBuilder.ExtractUserId(message.Receiver);
            if (string.IsNullOrWhiteSpace(recipientId))
            {
                throw new MessagingException(FacebookErrorCodes.InvalidRecipient,
                    Schema.ChannelType,
                    "Recipient User ID is required and must be a valid Facebook PSID");
            }

            // Build Facebook message request with Graph API validation
            var request = FacebookMessageBuilder.BuildMessageRequest(message, recipientId, Logger);

            // Send the message with Facebook Graph API requirements
            var response = await _facebookService.SendMessageAsync(request, cancellationToken);

            Logger?.LogMessageSent(message.Id, response.MessageId);

            var result = new SendResult(message.Id, response.MessageId)
            {
                Status = MessageStatus.Sent,
                Timestamp = DateTime.UtcNow
            };

            // Add enhanced properties with Graph API information
            result.AdditionalData["FacebookMessageId"] = response.MessageId;
            result.AdditionalData["RecipientId"] = response.RecipientId;
            result.AdditionalData["PageId"] = _pageId ?? "";
            result.AdditionalData["HttpClient"] = "RestSharp";
            result.AdditionalData["ApiVersion"] = FacebookConnectorConstants.GraphApiVersion;

            return result;
        }

        /// <inheritdoc/>
        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            var statusText = $"Facebook Messenger Connector (Page: {_pageId})";
            var statusInfo = new StatusInfo("Ready", statusText);

            statusInfo.AdditionalData["PageId"] = _pageId ?? "";
            statusInfo.AdditionalData["State"] = State.ToString();
            statusInfo.AdditionalData["Uptime"] = DateTime.UtcNow - _startTime;
            statusInfo.AdditionalData["ApiVersion"] = FacebookConnectorConstants.GraphApiVersion;
            statusInfo.AdditionalData["GraphApiCompliance"] = "Full";

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

            // Add RestSharp and Graph API specific health metrics
            health.Metrics["ApiVersion"] = FacebookConnectorConstants.GraphApiVersion;
            health.Metrics["PageId"] = _pageId ?? "";
            health.Metrics["GraphApiCompliance"] = "Full";

            if (State == ConnectorState.Ready)
            {
                try
                {
                    // Test connectivity by fetching page info using Graph API
                    await TestConnectorConnectionAsync(cancellationToken);

                    health.Metrics["LastSuccessfulApiCall"] = DateTime.UtcNow;
                } catch (Exception ex)
                {
                    health.IsHealthy = false;
                    health.Issues.Add($"Health check failed: {ex.Message}");
                }
            } else
            {
                health.IsHealthy = false;
                health.Issues.Add($"Health check failed: Connector is in {State} state");
            }

            return health;
        }

        /// <inheritdoc/>
        protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source,
            CancellationToken cancellationToken)
        {
            if (source.ContentType != MessageSource.JsonContentType)
            {
                throw new ConnectorException(FacebookErrorCodes.UnsupportedContentType,
                    Schema.ChannelType,
                    $"Unsupported content type: {source.ContentType}. Only application/json is supported for Facebook webhooks.");
            }

            var messages = FacebookMessageParser.ParseWebhook(source, _pageId);

            if (messages.Count == 0)
            {
                throw new ConnectorException(FacebookErrorCodes.InvalidWebhookData,
                    Schema.ChannelType,
                    "No valid messages found in webhook data");
            }

            return Task.FromResult(new ReceiveResult(Guid.NewGuid().ToString(), messages));
        }


        /// <inheritdoc/>
        protected override async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Don't validate recipient here - let EmailAddress pass so we can catch it in SendMessageCoreAsync
            // This allows us to return INVALID_RECIPIENT instead of MESSAGE_VALIDATION_FAILED

            // Run base validation for other checks
            var hasErrors = false;
            await foreach (var result in base.ValidateMessageCoreAsync(message, cancellationToken))
            {
                if (result != ValidationResult.Success)
                {
                    hasErrors = true;
                    yield return result;
                }
            }

            // Facebook-specific validations (except recipient which we handle in core method)
            if (message.Content?.ContentType == MessageContentType.PlainText && message.Content is ITextContent textContent)
            {
                if (!string.IsNullOrEmpty(textContent.Text) && textContent.Text.Length > 2000)
                {
                    hasErrors = true;
                    yield return new ValidationResult("Message text exceeds Facebook's 2000 character limit");
                }
            }

            // If no validation errors were found, yield success
            if (!hasErrors)
            {
                yield return ValidationResult.Success!;
            }
        }
    }
}
