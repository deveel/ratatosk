//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Polly;
using Polly.Retry;

using System.Net;
using System.Text;
using System.Text.Json;

namespace Ratatosk
{
    /// <summary>
    /// Default implementation of <see cref="IFacebookService"/> that communicates
    /// with the Facebook Graph API to send messages and fetch page information.
    /// </summary>
    public class FacebookService : IFacebookService
    {
        private readonly HttpClient _httpClient;
        private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;
        private string? _pageAccessToken;

        /// <summary>
        /// Constructs the service with an optional HTTP client.
        /// </summary>
        /// <param name="httpClient">
        /// An optional HTTP client used to communicate with the Facebook Graph API.
        /// If not provided, a new instance is created with the base URL set to the Graph API.
        /// </param>
        public FacebookService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(FacebookConnectorConstants.GraphApiBaseUrl) };
            _resiliencePipeline = CreateDefaultResiliencePipeline();
        }

        private static ResiliencePipeline<HttpResponseMessage> CreateDefaultResiliencePipeline()
        {
            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .HandleResult(r => (int)r.StatusCode >= 500)
                })
                .AddTimeout(TimeSpan.FromSeconds(30))
                .Build();
        }

        /// <summary>
        /// Initializes the service with a Facebook Page Access Token.
        /// </summary>
        /// <param name="pageAccessToken">The page access token to use for API requests.</param>
        public void Initialize(string pageAccessToken)
        {
            if (string.IsNullOrWhiteSpace(pageAccessToken))
                throw new ArgumentNullException(nameof(pageAccessToken), "Page Access Token cannot be null or empty");

            ValidateTokenFormat(pageAccessToken);

            _pageAccessToken = pageAccessToken;
        }

        /// <summary>
        /// Fetches information about a Facebook page using the Graph API.
        /// </summary>
        /// <param name="pageId">The identifier of the Facebook page.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>A <see cref="FacebookPageInfo"/> containing page details, or <c>null</c> if not found.</returns>
        public async Task<FacebookPageInfo?> FetchPageAsync(string pageId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_pageAccessToken))
                throw new InvalidOperationException("Facebook service has not been initialized with an access token.");

            if (string.IsNullOrWhiteSpace(pageId))
                throw new ArgumentException("Page ID cannot be null or empty", nameof(pageId));

            try
            {
                var url = $"/{FacebookConnectorConstants.GraphApiVersion}/{Uri.EscapeDataString(pageId)}?fields={Uri.EscapeDataString("id,name,category,access_token")}&access_token={Uri.EscapeDataString(_pageAccessToken)}";

                var response = await _resiliencePipeline.ExecuteAsync(
                    async ct => await _httpClient.GetAsync(url, ct),
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = ParseFacebookError(content, response.StatusCode);
                    var (graphApiCode, _) = ParseFacebookErrorCode(content);
                    var errorCode = MapGraphApiErrorCode(graphApiCode);

                    throw new ConnectorException(
                        errorCode,
                        FacebookErrorCodes.ErrorDomain,
                        $"Facebook Graph API error: {errorDetails}");
                }

                if (string.IsNullOrEmpty(content))
                    return null;

                var pageData = JsonSerializer.Deserialize<JsonElement>(content);

                return new FacebookPageInfo
                {
                    Id = GetJsonStringProperty(pageData, "id") ?? pageId,
                    Name = GetJsonStringProperty(pageData, "name") ?? "",
                    Category = GetJsonStringProperty(pageData, "category") ?? ""
                };
            }
            catch (Exception ex) when (!(ex is ConnectorException))
            {
                throw new ConnectorException(FacebookErrorCodes.ConnectionTestFailed, FacebookErrorCodes.ErrorDomain, $"Error fetching Facebook page: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a message to a Facebook recipient using the Graph API.
        /// </summary>
        /// <param name="request">The message request containing recipient, content, and options.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>A <see cref="FacebookMessageResponse"/> containing the result of the send operation.</returns>
        public async Task<FacebookMessageResponse> SendMessageAsync(FacebookMessageRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_pageAccessToken))
                throw new InvalidOperationException("Facebook service has not been initialized with an access token.");

            ArgumentNullException.ThrowIfNull(request, nameof(request));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Recipient, nameof(request.Recipient));
            ArgumentNullException.ThrowIfNull(request.Message, nameof(request.Message));

            ValidateMessageRequest(request);

            try
            {
                var url = $"/{FacebookConnectorConstants.GraphApiVersion}/me/messages?access_token={Uri.EscapeDataString(_pageAccessToken)}";

                var messagePayload = BuildFacebookMessagePayload(request);
                var json = JsonSerializer.Serialize(messagePayload);

                var response = await _resiliencePipeline.ExecuteAsync(async ct =>
                {
                    using var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync(url, requestContent, ct);
                }, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = ParseFacebookError(responseContent, response.StatusCode);
                    var (graphApiCode, _) = ParseFacebookErrorCode(responseContent);
                    var errorCode = MapGraphApiErrorCode(graphApiCode);

                    throw new ConnectorException(
                        errorCode,
                        FacebookErrorCodes.ErrorDomain,
                        $"Facebook Graph API error: {errorDetails}");
                }

                if (string.IsNullOrEmpty(responseContent))
                    throw new ConnectorException(FacebookErrorCodes.GraphApiError, FacebookErrorCodes.ErrorDomain, "Facebook API returned empty response");

                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return new FacebookMessageResponse
                {
                    MessageId = GetJsonStringProperty(responseData, "message_id") ?? "",
                    RecipientId = request.Recipient
                };
            }
            catch (Exception ex) when (!(ex is ConnectorException || ex is ArgumentException))
            {
                throw new ConnectorException(FacebookErrorCodes.GraphApiError, FacebookErrorCodes.ErrorDomain, $"Error sending Facebook message: {ex.Message}", ex);
            }
        }

        private static void ValidateTokenFormat(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Page Access Token cannot be null or empty", nameof(token));

            if (token.Length < 20)
                throw new ArgumentException($"Page Access Token is too short ({token.Length} characters). Minimum length is 20.", nameof(token));

            if (token.Contains(" "))
                throw new ArgumentException("Page Access Token contains spaces, which is not allowed", nameof(token));
        }

        private static void ValidateMessageRequest(FacebookMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message.Text) && request.Message.Attachment == null)
                throw new ArgumentException("Message must contain either text or attachment");

            if (!string.IsNullOrEmpty(request.Message.Text) && request.Message.Text.Length > 2000)
                throw new ArgumentException("Message text cannot exceed 2000 characters (Facebook limit)");

            if (request.Message.QuickReplies?.Count > 13)
                throw new ArgumentException("Maximum 13 quick replies allowed (Facebook limit)");

            var validMessagingTypes = new[] { "RESPONSE", "UPDATE", "MESSAGE_TAG", "NON_PROMOTIONAL_SUBSCRIPTION" };
            if (!validMessagingTypes.Contains(request.MessagingType))
                throw new ArgumentException($"Invalid messaging type. Must be one of: {string.Join(", ", validMessagingTypes)}");

            var validNotificationTypes = new[] { "REGULAR", "SILENT_PUSH", "NO_PUSH" };
            if (!string.IsNullOrEmpty(request.NotificationType) && !validNotificationTypes.Contains(request.NotificationType))
                throw new ArgumentException($"Invalid notification type. Must be one of: {string.Join(", ", validNotificationTypes)}");
        }

        internal static object BuildFacebookMessagePayload(FacebookMessageRequest request)
        {
            var payload = new Dictionary<string, object>
            {
                ["recipient"] = new { id = request.Recipient },
                ["messaging_type"] = request.MessagingType,
                ["message"] = BuildMessageContent(request.Message)
            };

            if (!string.IsNullOrEmpty(request.NotificationType) && request.NotificationType != "REGULAR")
            {
                payload["notification_type"] = request.NotificationType;
            }

            if (!string.IsNullOrEmpty(request.Tag))
            {
                payload["tag"] = request.Tag;
            }

            return payload;
        }

        internal static object BuildMessageContent(FacebookMessage message)
        {
            var content = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(message.Text))
            {
                content["text"] = message.Text;
            }

            if (message.Attachment != null)
            {
                content["attachment"] = new
                {
                    type = message.Attachment.Type,
                    payload = new
                    {
                        url = message.Attachment.Payload.Url,
                        is_reusable = message.Attachment.Payload.IsReusable
                    }
                };
            }
            else if (message.Template != null)
            {
                content["attachment"] = new
                {
                    type = "template",
                    payload = message.Template.Payload
                };
            }

            if (message.QuickReplies != null && message.QuickReplies.Count > 0)
            {
                content["quick_replies"] = message.QuickReplies.Select(qr =>
                {
                    var quickReply = new Dictionary<string, object>
                    {
                        ["content_type"] = qr.ContentType,
                        ["title"] = qr.Title,
                        ["payload"] = qr.Payload
                    };

                    if (!string.IsNullOrEmpty(qr.ImageUrl))
                    {
                        quickReply["image_url"] = qr.ImageUrl;
                    }

                    return quickReply;
                }).ToArray();
            }

            return content;
        }

        internal static (int? code, int? subcode) ParseFacebookErrorCode(string? content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                    return (null, null);

                var errorData = JsonSerializer.Deserialize<JsonElement>(content);

                if (errorData.TryGetProperty("error", out var error))
                {
                    var code = error.TryGetProperty("code", out var codeProp) ? codeProp.GetInt32() : (int?)null;
                    var subcode = error.TryGetProperty("error_subcode", out var subcodeProp) ? subcodeProp.GetInt32() : (int?)null;

                    return (code, subcode);
                }

                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }

        internal static string MapGraphApiErrorCode(int? graphApiCode)
        {
            return graphApiCode switch
            {
                1 or 2 => FacebookErrorCodes.GraphApiError,
                4 => FacebookErrorCodes.GraphApiError,
                100 => FacebookErrorCodes.GraphApiError,
                190 => FacebookErrorCodes.InvalidAccessToken,
                200 => FacebookErrorCodes.GraphApiError,
                368 => FacebookErrorCodes.GraphApiError,
                _ => FacebookErrorCodes.GraphApiError
            };
        }

        internal static string ParseFacebookError(string? content, HttpStatusCode statusCode)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                    return $"HTTP {(int)statusCode} {statusCode}: Unknown error";

                var errorData = JsonSerializer.Deserialize<JsonElement>(content);

                if (errorData.TryGetProperty("error", out var error))
                {
                    var message = GetJsonStringProperty(error, "message") ?? "Unknown error";
                    var code = GetJsonStringProperty(error, "code") ?? "";
                    var subcode = GetJsonStringProperty(error, "error_subcode") ?? "";

                    return !string.IsNullOrEmpty(subcode)
                        ? $"Code {code} (Subcode {subcode}): {message}"
                        : $"Code {code}: {message}";
                }

                return content;
            }
            catch
            {
                return !string.IsNullOrEmpty(content)
                    ? content
                    : $"HTTP {(int)statusCode} {statusCode}: Unknown error";
            }
        }

        internal static string? GetJsonStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
        }
    }
}
