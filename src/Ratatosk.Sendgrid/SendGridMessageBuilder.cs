//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using SendGrid.Helpers.Mail;

using System.Text.Json;

namespace Ratatosk
{
    /// <summary>
    /// Provides methods for building and configuring <see cref="SendGridMessage"/> objects
    /// from <see cref="IMessage"/> instances and connector settings.
    /// </summary>
    internal sealed class SendGridMessageBuilder
    {
        private readonly bool _sandboxMode;
        private readonly bool _trackingSettings;
        private readonly string? _defaultReplyTo;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridMessageBuilder"/> class.
        /// </summary>
        /// <param name="sandboxMode">Whether to enable sandbox mode for testing.</param>
        /// <param name="trackingSettings">Whether to enable click and open tracking.</param>
        /// <param name="defaultReplyTo">The default reply-to email address.</param>
        /// <param name="logger">An optional logger for diagnostic messages.</param>
        public SendGridMessageBuilder(
            bool sandboxMode,
            bool trackingSettings,
            string? defaultReplyTo,
            ILogger? logger = null)
        {
            _sandboxMode = sandboxMode;
            _trackingSettings = trackingSettings;
            _defaultReplyTo = defaultReplyTo;
            _logger = logger;
        }

        /// <summary>
        /// Sets the email content on the <see cref="SendGridMessage"/> based on the
        /// <see cref="IMessage.Content"/> type.
        /// </summary>
        /// <param name="sendGridMessage">The SendGrid message to configure.</param>
        /// <param name="message">The source message containing the content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetMessageContentAsync(SendGridMessage sendGridMessage, IMessage message)
        {
            if (message.Content == null)
                return Task.CompletedTask;

            switch (message.Content.ContentType)
            {
                case MessageContentType.PlainText when message.Content is ITextContent textContent:
                    sendGridMessage.PlainTextContent = textContent.Text;
                    break;

                case MessageContentType.Html when message.Content is IHtmlContent htmlContent:
                    sendGridMessage.HtmlContent = htmlContent.Html;
                    break;

                case MessageContentType.Multipart when message.Content is IMultipartContent multipartContent:
                    foreach (var part in multipartContent.Parts)
                    {
                        if (part.ContentType == MessageContentType.PlainText && part is ITextContent textPart)
                            sendGridMessage.PlainTextContent = textPart.Text;
                        else if (part.ContentType == MessageContentType.Html && part is IHtmlContent htmlPart)
                            sendGridMessage.HtmlContent = htmlPart.Html;
                    }
                    break;

                case MessageContentType.Template when message.Content is ITemplateContent templateContent:
                    sendGridMessage.TemplateId = templateContent.TemplateId;
                    if (templateContent.Parameters != null && templateContent.Parameters.Any())
                        sendGridMessage.SetTemplateData(templateContent.Parameters);
                    break;

                default:
                    if (message.Content is ITextContent fallbackText)
                        sendGridMessage.PlainTextContent = fallbackText.Text;
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Applies message-level properties (categories, custom args, priority, etc.) to the
        /// <see cref="SendGridMessage"/>.
        /// </summary>
        /// <param name="sendGridMessage">The SendGrid message to configure.</param>
        /// <param name="message">The source message containing property values.</param>
        /// <param name="properties">The dictionary of message properties to apply.</param>
        public void ApplyMessageSettings(SendGridMessage sendGridMessage, IMessage message, Dictionary<string, object?> properties)
        {
            if (properties.TryGetValue("Priority", out var priorityValue))
            {
                var priority = priorityValue?.ToString()?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(priority))
                {
                    var priorityNum = priority switch
                    {
                        "high" => "1",
                        "normal" => "3",
                        "low" => "5",
                        _ => "3"
                    };
                    sendGridMessage.AddHeader("X-Priority", priorityNum);
                }
            }

            if (properties.TryGetValue("Categories", out var categoriesValue))
            {
                var categories = categoriesValue?.ToString();
                if (!string.IsNullOrEmpty(categories))
                {
                    var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();
                    sendGridMessage.Categories = categoryList;
                }
            }

            if (properties.TryGetValue("CustomArgs", out var customArgsValue))
            {
                var customArgs = customArgsValue?.ToString();
                if (!string.IsNullOrEmpty(customArgs))
                {
                    try
                    {
                        var argsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(customArgs);
                        if (argsDict != null)
                            sendGridMessage.CustomArgs = argsDict;
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogCustomArgsParseFailed(customArgs, ex);
                    }
                }
            }

            if (properties.TryGetValue("SendAt", out var sendAtValue))
            {
                DateTime sendAt;
                if (sendAtValue is DateTime dateTime)
                    sendAt = dateTime;
                else if (sendAtValue is string dateString && DateTime.TryParse(dateString, out var parsedDate))
                    sendAt = parsedDate;
                else
                    goto skipSendAt;

                sendGridMessage.SendAt = ((DateTimeOffset)sendAt.ToUniversalTime()).ToUnixTimeSeconds();
            }
            skipSendAt:

            if (properties.TryGetValue("BatchId", out var batchIdValue))
            {
                var batchId = batchIdValue?.ToString();
                if (!string.IsNullOrEmpty(batchId))
                    sendGridMessage.BatchId = batchId;
            }

            if (properties.TryGetValue("AsmGroupId", out var asmGroupIdValue))
            {
                if (int.TryParse(asmGroupIdValue?.ToString(), out var asmGroupId))
                    sendGridMessage.Asm = new ASM { GroupId = asmGroupId };
            }

            if (properties.TryGetValue("IpPoolName", out var ipPoolValue))
            {
                var ipPoolName = ipPoolValue?.ToString();
                if (!string.IsNullOrEmpty(ipPoolName))
                    sendGridMessage.IpPoolName = ipPoolName;
            }

            if (!string.IsNullOrEmpty(_defaultReplyTo))
                sendGridMessage.ReplyTo = new EmailAddress(_defaultReplyTo);
        }

        /// <summary>
        /// Applies connector-level settings (sandbox mode, tracking) to the <see cref="SendGridMessage"/>.
        /// </summary>
        /// <param name="sendGridMessage">The SendGrid message to configure.</param>
        public void ApplyConnectorSettings(SendGridMessage sendGridMessage)
        {
            if (_sandboxMode)
            {
                sendGridMessage.MailSettings = sendGridMessage.MailSettings ?? new MailSettings();
                sendGridMessage.MailSettings.SandboxMode = new SandboxMode { Enable = true };
            }

            if (_trackingSettings)
            {
                sendGridMessage.TrackingSettings = sendGridMessage.TrackingSettings ?? new TrackingSettings();
                sendGridMessage.TrackingSettings.ClickTracking = new ClickTracking { Enable = true };
                sendGridMessage.TrackingSettings.OpenTracking = new OpenTracking { Enable = true };
            }
        }
    }
}

