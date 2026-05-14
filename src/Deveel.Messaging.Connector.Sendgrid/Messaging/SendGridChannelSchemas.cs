//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides pre-configured channel schemas for SendGrid email messaging services.
    /// </summary>
    public static class SendGridChannelSchemas
    {
        /// <summary>
        /// Gets the comprehensive base schema for SendGrid email messaging that supports
        /// all available capabilities and configurations.
        /// </summary>
        /// <remarks>
        /// This schema includes all SendGrid email capabilities including sending,
        /// status queries, templates, and webhook support. It can be used as-is
        /// or derived to create more restrictive configurations for specific use cases.
        /// </remarks>
        public static ChannelSchema SendGridEmail => new ChannelSchemaBuilder(SendGridConnectorConstants.Provider, SendGridConnectorConstants.EmailChannel, "1.0.0")
            .WithDisplayName("SendGrid Email Connector")
            .WithCapabilities(
                ChannelCapability.SendMessages | 
                ChannelCapability.ReceiveMessages |
                ChannelCapability.MessageStatusQuery |
                ChannelCapability.HandleMessageState |
                ChannelCapability.BulkMessaging |
                ChannelCapability.Templates |
                ChannelCapability.MediaAttachments |
                ChannelCapability.HealthCheck)
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.ApiKey, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "SendGrid API Key - found in your SendGrid Dashboard under Settings > API Keys"
            })
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.SandboxMode, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = SendGridConnectionSettingsDefaults.SandboxMode,
                Description = "Enable sandbox mode for testing without actually sending emails"
            })
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.WebhookUrl, DataType.String)
            {
                IsRequired = false,
                Description = "URL to receive webhook notifications for email events and status updates"
            })
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.TrackingSettings, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = SendGridConnectionSettingsDefaults.TrackingSettings,
                Description = "Enable tracking for opens, clicks, and other email engagement metrics"
            })
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.DefaultFromName, DataType.String)
            {
                IsRequired = false,
                Description = "Default sender name to use when not specified in the message"
            })
            .AddParameter(new ChannelParameter(SendGridConnectionParameters.DefaultReplyTo, DataType.String)
            {
                IsRequired = false,
                Description = "Default reply-to email address"
            })
            .AddContentType(MessageContentType.PlainText)
            .AddContentType(MessageContentType.Html)
            .AddContentType(MessageContentType.Template)
            .AddContentType(MessageContentType.Multipart)
            .HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
            {
                e.CanSend = true;
                e.CanReceive = true; // Email addresses can be both senders and receivers
                e.IsRequired = true; // Email address is required for sending
            })
            .HandlesMessageEndpoint(EndpointType.Url, e =>
            {
                e.CanSend = false;
                e.CanReceive = true; // For webhooks
            })
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "SendGrid API Key")
                .WithField(SendGridConnectionParameters.ApiKey, DataType.String, f =>
                {
                    f.DisplayName = "API Key";
                    f.Description = "SendGrid API Key from your SendGrid Dashboard";
                    f.AuthenticationRole = "principal";
                    f.IsSensitive = true;
                }))
            .AddMessageProperty("Subject", DataType.String, p =>
            {
                p.IsRequired = true;
                p.Description = "Email subject line";
                p.MaxLength = 998; // RFC 2822 limit
                p.CustomValidator = value =>
                {
                    if (value != null && string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        return new[]
                        {
                            new ValidationResult(
                                "Subject cannot be empty",
                                new[] { "Subject" })
                        };
                    }
                    return Enumerable.Empty<ValidationResult>();
                };
			})
            .AddMessageProperty("Priority", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Email priority (low, normal, high)";
                p.AllowedValues = new[] { "low", "normal", "high" };
			})
            .AddMessageProperty("Categories", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Comma-separated list of categories for tracking and organization";
                p.CustomValidator = ValidateCategories;
            })
            .AddMessageProperty("CustomArgs", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "JSON object containing custom arguments to attach to the email";
                p.CustomValidator = ValidateJsonContent;
            })
            .AddMessageProperty("SendAt", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Schedule the email to be sent at a specific time (ISO 8601 format or DateTime)";
                p.CustomValidator = ValidateSendAtTime;
            })
            .AddMessageProperty("BatchId", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Batch ID for grouping emails together for batch operations";
            })
            .AddMessageProperty("IpPoolName", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "IP pool name to use for sending this email";
            })
            .AddMessageProperty("AsmGroupId", DataType.Integer, p =>
            {
                p.IsRequired = false;
                p.Description = "Unsubscribe group ID for subscription management";
                p.MinValue = 1; // SendGrid requires group IDs to be positive integers
			})
            .Build();

        /// <summary>
        /// Gets a simplified email-only schema for basic email messaging use cases.
        /// This schema removes webhooks, templates, and advanced features.
        /// </summary>
        public static ChannelSchema SimpleEmail => ChannelSchemaBuilder.From(SendGridEmail, "SendGrid Simple Email")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .RemoveCapability(ChannelCapability.HandleMessageState)
            .RemoveCapability(ChannelCapability.BulkMessaging)
            .RemoveCapability(ChannelCapability.Templates)
            .RemoveCapability(ChannelCapability.MediaAttachments)
            .RemoveParameter(SendGridConnectionParameters.WebhookUrl)
            .RemoveParameter(SendGridConnectionParameters.TrackingSettings)
            .RemoveContentType(MessageContentType.Template)
            .RemoveContentType(MessageContentType.Multipart)
            .RemoveMessageProperty("Categories")
            .RemoveMessageProperty("CustomArgs")
            .RemoveMessageProperty("SendAt")
            .RemoveMessageProperty("BatchId")
            .RemoveMessageProperty("IpPoolName")
            .RemoveMessageProperty("AsmGroupId")
            .Build();

        /// <summary>
        /// Gets a transactional email schema optimized for automated notifications and receipts.
        /// This schema includes tracking and delivery confirmation but removes bulk capabilities.
        /// </summary>
        public static ChannelSchema TransactionalEmail => ChannelSchemaBuilder.From(SendGridEmail, "SendGrid Transactional Email")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .RemoveCapability(ChannelCapability.HandleMessageState)
            .RemoveCapability(ChannelCapability.BulkMessaging)
            .RemoveCapability(ChannelCapability.Templates)
            .RemoveParameter(SendGridConnectionParameters.WebhookUrl)
            .UpdateParameter(SendGridConnectionParameters.TrackingSettings, param => param.DefaultValue = SendGridConnectionSettingsDefaults.TrackingSettings)
            .RemoveContentType(MessageContentType.Template)
            .RemoveMessageProperty("SendAt")
            .RemoveMessageProperty("BatchId")
            .RemoveMessageProperty("IpPoolName")
            .Build();

        /// <summary>
        /// Gets a marketing email schema optimized for campaigns and newsletters.
        /// This schema includes all tracking, templates, and bulk messaging capabilities.
        /// </summary>
        public static ChannelSchema MarketingEmail => ChannelSchemaBuilder.From(SendGridEmail, "SendGrid Marketing Email")
            .UpdateParameter(SendGridConnectionParameters.TrackingSettings, param => param.DefaultValue = SendGridConnectionSettingsDefaults.TrackingSettings)
            .AddMessageProperty("ListId", DataType.String, p =>
            {
                p.Description = "Marketing list ID for campaign tracking";
            })
            .AddMessageProperty("CampaignId", DataType.String, p =>
            {
                p.Description = "Campaign ID for grouping and tracking marketing emails";
            })
            .Build();

        /// <summary>
        /// Gets a template-focused email schema optimized for dynamic content using SendGrid templates.
        /// This schema focuses on template messaging capabilities with webhook support for tracking.
        /// </summary>
        public static ChannelSchema TemplateEmail => ChannelSchemaBuilder.From(SendGridEmail, "SendGrid Template Email")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .RemoveCapability(ChannelCapability.HandleMessageState)
            .RemoveCapability(ChannelCapability.MediaAttachments)
            .RemoveContentType(MessageContentType.PlainText)
            .RemoveContentType(MessageContentType.Html)
            .RemoveContentType(MessageContentType.Multipart)
            .AddMessageProperty("TemplateId", DataType.String, p =>
            {
                p.IsRequired = true;
                p.Description = "SendGrid template ID to use for the email";
                p.MinLength = 1;
            })
            .AddMessageProperty("TemplateData", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "JSON object containing template variable substitutions";
                p.CustomValidator = ValidateJsonContent;
            })
            .Build();

        /// <summary>
        /// Gets a bulk email schema optimized for high-volume email campaigns.
        /// This schema includes batch processing and advanced delivery options.
        /// </summary>
        public static ChannelSchema BulkEmail => ChannelSchemaBuilder.From(SendGridEmail, "SendGrid Bulk Email")
            .UpdateParameter(SendGridConnectionParameters.TrackingSettings, param => param.DefaultValue = SendGridConnectionSettingsDefaults.TrackingSettings)
            .AddMessageProperty("MailBatchId", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Mail batch ID for bulk operations and tracking";
            })
            .AddMessageProperty("UnsubscribeGroupId", DataType.Integer, p =>
            {
                p.IsRequired = false;
                p.Description = "Unsubscribe group ID for bulk email compliance";
                p.MinValue = 1;
            })
            .Build();

        /// <summary>
        /// Validates that categories property contains at most 10 categories with max 255 chars each.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateCategories(object? value)
        {
            if (value == null) yield break;

            var categories = value.ToString();
            if (string.IsNullOrEmpty(categories)) yield break;

            var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            if (categoryList.Length > 10)
            {
                yield return new ValidationResult(
                    "Cannot specify more than 10 categories",
                    new[] { "Categories" });
            }

            foreach (var category in categoryList)
            {
                if (category.Trim().Length > 255)
                {
                    yield return new ValidationResult(
                        "Category name cannot exceed 255 characters",
                        new[] { "Categories" });
                    break;
                }
            }
        }

        /// <summary>
        /// Validates that the property contains valid JSON content.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateJsonContent(object? value)
        {
            if (value == null) 
                return Enumerable.Empty<ValidationResult>();

            var jsonContent = value.ToString();
            if (string.IsNullOrEmpty(jsonContent)) 
                return Enumerable.Empty<ValidationResult>();

            try
            {
                System.Text.Json.JsonDocument.Parse(jsonContent);
                return Enumerable.Empty<ValidationResult>();
            }
            catch (System.Text.Json.JsonException)
            {
                return new[]
                {
                    new ValidationResult(
                        "CustomArgs must be valid JSON",
                        new[] { "CustomArgs" })
                };
            }
        }

        /// <summary>
        /// Validates that SendAt time is in the future and within SendGrid's 72-hour limit.
        /// Handles both DateTime and string values.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateSendAtTime(object? value)
        {
            if (value == null) 
                yield break;

            DateTime sendAt;
            
            // Handle DateTime objects directly (don't treat them as type mismatches)
            if (value is DateTime dateTime)
            {
                sendAt = dateTime;
            }
            else if (value is string dateString)
            {
                if (!DateTime.TryParse(dateString, out sendAt))
                {
                    yield return new ValidationResult(
                        "SendAt must be a valid date and time",
                        new[] { "SendAt" });
                    yield break;
                }
            }
            else
            {
                yield return new ValidationResult(
                    "SendAt must be a valid date and time or DateTime object",
                    new[] { "SendAt" });
                yield break;
            }

            if (sendAt <= DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    "SendAt must be a future date and time",
                    new[] { "SendAt" });
            }
            else if (sendAt > DateTime.UtcNow.AddDays(72)) // SendGrid limit
            {
                yield return new ValidationResult(
                    "SendAt cannot be more than 72 hours in the future",
                    new[] { "SendAt" });
            }
        }
    }
}