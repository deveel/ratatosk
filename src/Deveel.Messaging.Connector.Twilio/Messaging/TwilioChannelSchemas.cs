//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides pre-configured channel schemas for Twilio messaging services,
    /// aligned with the current Twilio C# Helper Library (SDK v7.x).
    /// </summary>
    public static class TwilioChannelSchemas
    {
        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioSms"/>
        public static ChannelSchema TwilioSms => TwilioSchemaBuilder.CreateTwilioSms(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioWhatsApp"/>
        public static ChannelSchema TwilioWhatsApp => TwilioSchemaBuilder.CreateTwilioWhatsApp(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleSms"/>
        public static ChannelSchema SimpleSms => TwilioSchemaBuilder.CreateSimpleSms(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateNotificationSms"/>
        public static ChannelSchema NotificationSms => TwilioSchemaBuilder.CreateNotificationSms(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateBulkSms"/>
        public static ChannelSchema BulkSms => TwilioSchemaBuilder.CreateBulkSms(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleWhatsApp"/>
        public static ChannelSchema SimpleWhatsApp => TwilioSchemaBuilder.CreateSimpleWhatsApp(TwilioConnectorConstants.ConnectorSchemaVersion);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateWhatsAppTemplates"/>
        public static ChannelSchema WhatsAppTemplates => TwilioSchemaBuilder.CreateWhatsAppTemplates(TwilioConnectorConstants.ConnectorSchemaVersion);
    }

    /// <summary>
    /// Provides Twilio channel schemas aligned with SDK v6.x capabilities.
    /// Includes Messaging Services and WhatsApp Business API support.
    /// </summary>
    public static class TwilioChannelSchemasV6
    {
        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioSms"/>
        public static ChannelSchema TwilioSms => TwilioSchemaBuilder.CreateTwilioSms(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioWhatsApp"/>
        public static ChannelSchema TwilioWhatsApp => TwilioSchemaBuilder.CreateTwilioWhatsApp(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleSms"/>
        public static ChannelSchema SimpleSms => TwilioSchemaBuilder.CreateSimpleSms(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateNotificationSms"/>
        public static ChannelSchema NotificationSms => TwilioSchemaBuilder.CreateNotificationSms(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateBulkSms"/>
        public static ChannelSchema BulkSms => TwilioSchemaBuilder.CreateBulkSms(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleWhatsApp"/>
        public static ChannelSchema SimpleWhatsApp => TwilioSchemaBuilder.CreateSimpleWhatsApp(TwilioConnectorConstants.SdkVersion6);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateWhatsAppTemplates"/>
        public static ChannelSchema WhatsAppTemplates => TwilioSchemaBuilder.CreateWhatsAppTemplates(TwilioConnectorConstants.SdkVersion6);
    }

    /// <summary>
    /// Provides Twilio channel schemas aligned with SDK v5.x capabilities.
    /// Covers basic Programmable SMS and early WhatsApp support without Messaging Services.
    /// </summary>
    public static class TwilioChannelSchemasV5
    {
        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioSms"/>
        public static ChannelSchema TwilioSms => TwilioSchemaBuilder.CreateTwilioSms(TwilioConnectorConstants.SdkVersion5);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateTwilioWhatsApp"/>
        public static ChannelSchema TwilioWhatsApp => TwilioSchemaBuilder.CreateTwilioWhatsApp(TwilioConnectorConstants.SdkVersion5);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleSms"/>
        public static ChannelSchema SimpleSms => TwilioSchemaBuilder.CreateSimpleSms(TwilioConnectorConstants.SdkVersion5);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateNotificationSms"/>
        public static ChannelSchema NotificationSms => TwilioSchemaBuilder.CreateNotificationSms(TwilioConnectorConstants.SdkVersion5);

        /// <inheritdoc cref="TwilioSchemaBuilder.CreateSimpleWhatsApp"/>
        public static ChannelSchema SimpleWhatsApp => TwilioSchemaBuilder.CreateSimpleWhatsApp(TwilioConnectorConstants.SdkVersion5);
        // BulkSms and WhatsAppTemplates are not available in SDK v5.x:
        // BulkSms requires MessagingServiceSid (introduced in v6.0).
        // WhatsAppTemplates requires WhatsApp template capability (introduced in v6.0).
    }

    internal static class TwilioSchemaBuilder
    {
        /// <summary>
        /// Creates the full base SMS schema for the given SDK version.
        /// </summary>
        /// <remarks>
        /// Feature availability by version:
        /// <list type="bullet">
        ///   <item>v5.0: Basic SMS (ValidityPeriod, MaxPrice, text and MMS content).</item>
        ///   <item>v6.0: Adds MessagingServiceSid parameter.</item>
        ///   <item>v7.0: Adds AttemptLimits, SmartEncoded, and PersistentAction message properties.</item>
        /// </list>
        /// </remarks>
        internal static ChannelSchema CreateTwilioSms(string sdkVersion)
        {
            sdkVersion = NormalizeSupportedVersion(sdkVersion);

            var schema = new ChannelSchema(TwilioConnectorConstants.Provider, TwilioConnectorConstants.SmsChannel, sdkVersion)
                .WithDisplayName("Twilio SMS Connector")
                .WithCapabilities(
                    ChannelCapability.SendMessages |
                    ChannelCapability.ReceiveMessages |
                    ChannelCapability.MessageStatusQuery |
                    ChannelCapability.HandleMessageState |
                    ChannelCapability.BulkMessaging |
                    ChannelCapability.HealthCheck)
                .AddParameter(new ChannelParameter("AccountSid", DataType.String)
                {
                    IsRequired = true,
                    Description = "Twilio Account SID - found in your Twilio Console Dashboard"
                })
                .AddParameter(new ChannelParameter("AuthToken", DataType.String)
                {
                    IsRequired = true,
                    IsSensitive = true,
                    Description = "Twilio Auth Token - found in your Twilio Console Dashboard"
                })
                .AddParameter(new ChannelParameter("WebhookUrl", DataType.String)
                {
                    IsRequired = false,
                    Description = "URL to receive webhook notifications for message status updates and incoming messages"
                })
                .AddParameter(new ChannelParameter("StatusCallback", DataType.String)
                {
                    IsRequired = false,
                    Description = "URL to receive delivery status callbacks for sent messages"
                })
                .AddParameter(new ChannelParameter("ValidityPeriod", DataType.Integer)
                {
                    IsRequired = false,
                    DefaultValue = 14400, // 4 hours in seconds
                    Description = "The number of seconds that the message can remain in Twilio's outgoing message queue"
                })
                .AddParameter(new ChannelParameter("MaxPrice", DataType.Number)
                {
                    IsRequired = false,
                    Description = "The maximum price in US dollars that you are willing to pay for the message"
                })
                .AddContentType(MessageContentType.PlainText)
                .AddContentType(MessageContentType.Media)
                .HandlesMessageEndpoint(EndpointType.PhoneNumber, e =>
                {
                    e.CanSend = true;
                    e.CanReceive = true;
                    e.IsRequired = true;
                })
                .HandlesMessageEndpoint(EndpointType.Url, e =>
                {
                    e.CanSend = false;
                    e.CanReceive = true;
                })
                .AddAuthenticationType(AuthenticationType.Basic)
                .AddMessageProperty("ValidityPeriod", DataType.Integer, p =>
                {
                    p.IsRequired = false;
                    p.Description = "Message-specific validity period override";
                })
                .AddMessageProperty("MaxPrice", DataType.Number, p =>
                {
                    p.IsRequired = false;
                    p.Description = "Message-specific maximum price override";
                })
                .AddMessageProperty("ProvideCallback", DataType.Boolean, p =>
                {
                    p.IsRequired = false;
                    p.Description = "Whether to provide delivery status callbacks for this message";
                });

            // Messaging Services support introduced in SDK v6.0
            if (IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion6))
            {
                schema = schema.AddParameter(new ChannelParameter("MessagingServiceSid", DataType.String)
                {
                    IsRequired = false,
                    Description = "The SID of the Messaging Service to use for the message. Can replace Sender for sending."
                });
            }

            // AttemptLimits, SmartEncoded, and PersistentAction (RCS) introduced in SDK v7.0
            if (IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion))
            {
                schema = schema
                    .AddMessageProperty("AttemptLimits", DataType.Integer, p =>
                    {
                        p.IsRequired = false;
                        p.Description = "Total number of attempts made by Twilio to deliver the message";
                    })
                    .AddMessageProperty("SmartEncoded", DataType.Boolean, p =>
                    {
                        p.IsRequired = false;
                        p.Description = "Whether Twilio will automatically optimize the message encoding";
                    })
                    .AddMessageProperty("PersistentAction", DataType.String, p =>
                    {
                        p.IsRequired = false;
                        p.Description = "Rich Communication Services (RCS) specific action";
                    });
            }

            return schema;
        }

        /// <summary>
        /// Creates the full base WhatsApp schema for the given SDK version.
        /// </summary>
        /// <remarks>
        /// Feature availability by version:
        /// <list type="bullet">
        ///   <item>v5.0: Basic WhatsApp (text and media content only).</item>
        ///   <item>v6.0: Adds template capability and Template content type (WhatsApp Business API via whatsapp: prefix).</item>
        ///   <item>v7.0: Adds PersistentAction message property (RCS).</item>
        /// </list>
        /// </remarks>
        internal static ChannelSchema CreateTwilioWhatsApp(string sdkVersion)
        {
            sdkVersion = NormalizeSupportedVersion(sdkVersion);

            var hasTemplates = IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion6);

            var capabilities =
                ChannelCapability.SendMessages |
                ChannelCapability.ReceiveMessages |
                ChannelCapability.MessageStatusQuery |
                ChannelCapability.HandleMessageState |
                ChannelCapability.MediaAttachments |
                ChannelCapability.HealthCheck;

            if (hasTemplates)
                capabilities |= ChannelCapability.Templates;

            var schema = new ChannelSchema(TwilioConnectorConstants.Provider, TwilioConnectorConstants.WhatsAppChannel, sdkVersion)
                .WithDisplayName("Twilio WhatsApp Business API Connector")
                .WithCapabilities(capabilities)
                .AddParameter("AccountSid", DataType.String, p =>
                {
                    p.IsRequired = true;
                    p.Description = "Twilio Account SID - found in your Twilio Console Dashboard";
                })
                .AddParameter("AuthToken", DataType.String, p =>
                {
                    p.IsRequired = true;
                    p.IsSensitive = true;
                    p.Description = "Twilio Auth Token - found in your Twilio Console Dashboard";
                })
                .AddParameter("WebhookUrl", DataType.String, p =>
                {
                    p.IsRequired = false;
                    p.Description = "URL to receive webhook notifications for message status updates and incoming WhatsApp messages";
                })
                .AddParameter("StatusCallback", DataType.String, p =>
                {
                    p.IsRequired = false;
                    p.Description = "URL to receive delivery status callbacks for sent WhatsApp messages";
                })
                .AddContentType(MessageContentType.PlainText)
                .AddContentType(MessageContentType.Media)
                .HandlesMessageEndpoint(EndpointType.PhoneNumber, e =>
                {
                    e.CanSend = true;
                    e.CanReceive = true;
                    e.IsRequired = true;
                })
                .HandlesMessageEndpoint(EndpointType.Url, e =>
                {
                    e.CanSend = false;
                    e.CanReceive = true;
                })
                .AddAuthenticationType(AuthenticationType.Basic)
                .AddMessageProperty("ProvideCallback", DataType.Boolean, p =>
                {
                    p.IsRequired = false;
                    p.Description = "Whether to provide delivery status callbacks for this WhatsApp message";
                });

            // Template support introduced in SDK v6.0 (WhatsApp Business API via whatsapp: prefix)
            if (hasTemplates)
                schema = schema.AddContentType(MessageContentType.Template);

            // PersistentAction (RCS) introduced in SDK v7.0
            if (IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion))
            {
                schema = schema.AddMessageProperty("PersistentAction", DataType.String, p =>
                {
                    p.IsRequired = false;
                    p.Description = "Rich Communication Services (RCS) specific action for WhatsApp";
                });
            }

            return schema;
        }

        /// <summary>Creates a simplified send-only SMS schema without webhooks or advanced features.</summary>
        internal static ChannelSchema CreateSimpleSms(string sdkVersion) =>
            new ChannelSchema(CreateTwilioSms(sdkVersion), "Twilio Simple SMS")
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.HandleMessageState)
                .RemoveCapability(ChannelCapability.BulkMessaging)
                .RemoveParameter("WebhookUrl")
                .RemoveParameter("StatusCallback")
                .RemoveParameter("MessagingServiceSid") // no-op for v5.0
                .RemoveContentType(MessageContentType.Media)
                .RemoveMessageProperty("ProvideCallback")
                .RemoveMessageProperty("PersistentAction") // no-op for v5.0/v6.0
                .RemoveMessageProperty("SmartEncoded"); // no-op for v5.0/v6.0

        /// <summary>Creates a send-only schema optimized for notifications and alerts.</summary>
        internal static ChannelSchema CreateNotificationSms(string sdkVersion) =>
            new ChannelSchema(CreateTwilioSms(sdkVersion), "Twilio Notification SMS")
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.HandleMessageState)
                .RemoveParameter("WebhookUrl")
                .RemoveContentType(MessageContentType.Media)
                .RemoveMessageProperty("PersistentAction"); // no-op for v5.0/v6.0

        /// <summary>
        /// Creates a bulk messaging schema optimized for high-volume SMS campaigns via a Messaging Service.
        /// </summary>
        /// <remarks>Requires SDK v6.0 or higher (MessagingServiceSid support).</remarks>
        internal static ChannelSchema CreateBulkSms(string sdkVersion)
        {
            sdkVersion = NormalizeSupportedVersion(sdkVersion);

            if (!IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion6))
                throw new InvalidOperationException(
                    $"BulkSms schema requires SDK v{TwilioConnectorConstants.SdkVersion6} or higher. " +
                    $"MessagingServiceSid is not available in SDK v{sdkVersion}.");

            return new ChannelSchema(CreateTwilioSms(sdkVersion), "Twilio Bulk SMS")
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.HandleMessageState)
                .UpdateParameter("MessagingServiceSid", param => param.IsRequired = true)
                .UpdateEndpoint(EndpointType.PhoneNumber, endpoint =>
                {
                    endpoint.IsRequired = false; // Not required when MessagingServiceSid is used
                    endpoint.CanReceive = false; // Send-only
                })
                .RemoveMessageProperty("PersistentAction"); // no-op for v6.0
        }

        /// <summary>Creates a simplified send-only WhatsApp schema for basic text and media use cases.</summary>
        internal static ChannelSchema CreateSimpleWhatsApp(string sdkVersion) =>
            new ChannelSchema(CreateTwilioWhatsApp(sdkVersion), "Twilio Simple WhatsApp")
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.HandleMessageState)
                .RemoveCapability(ChannelCapability.Templates)
                .RemoveParameter("WebhookUrl")
                .RemoveParameter("StatusCallback")
                .RemoveContentType(MessageContentType.Template) // no-op for v5.0
                .RemoveMessageProperty("ProvideCallback")
                .RemoveMessageProperty("PersistentAction"); // no-op for v5.0/v6.0

        /// <summary>
        /// Creates a template-focused WhatsApp schema for business notifications using approved templates.
        /// </summary>
        /// <remarks>Requires SDK v6.0 or higher (WhatsApp template support).</remarks>
        internal static ChannelSchema CreateWhatsAppTemplates(string sdkVersion)
        {
            sdkVersion = NormalizeSupportedVersion(sdkVersion);

            if (!IsAtLeastVersion(sdkVersion, TwilioConnectorConstants.SdkVersion6))
                throw new InvalidOperationException(
                    $"WhatsAppTemplates schema requires SDK v{TwilioConnectorConstants.SdkVersion6} or higher. " +
                    $"WhatsApp template support is not available in SDK v{sdkVersion}.");

            return new ChannelSchema(CreateTwilioWhatsApp(sdkVersion), "Twilio WhatsApp Templates")
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.HandleMessageState)
                .RemoveCapability(ChannelCapability.MediaAttachments)
                .RemoveContentType(MessageContentType.Media);
        }

        private static bool IsAtLeastVersion(string sdkVersion, string minVersion) =>
            Version.TryParse(sdkVersion, out var v) &&
            Version.TryParse(minVersion, out var min) &&
            v >= min;

        private static string NormalizeSupportedVersion(string sdkVersion)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sdkVersion, nameof(sdkVersion));

            var matchingVersion = TwilioConnectorConstants.SupportedSchemaVersions
                .FirstOrDefault(v => string.Equals(v, sdkVersion, StringComparison.OrdinalIgnoreCase));

            return matchingVersion ?? throw new ArgumentException(
                $"Unsupported Twilio SDK schema version '{sdkVersion}'. " +
                $"Supported versions: {string.Join(", ", TwilioConnectorConstants.SupportedSchemaVersions)}",
                nameof(sdkVersion));
        }
    }
}
