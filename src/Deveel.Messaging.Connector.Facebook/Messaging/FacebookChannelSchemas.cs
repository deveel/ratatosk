//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides the default Facebook schemas aligned with the current Graph API version.
    /// </summary>
    public static class FacebookChannelSchemas
    {
    /// <summary>
    /// Gets the full Facebook Messenger schema for the default supported Graph API version.
    /// </summary>
        public static ChannelSchema FacebookMessenger => FacebookSchemaBuilder.CreateFacebookMessenger(FacebookConnectorConstants.ConnectorSchemaVersion);

    /// <summary>
    /// Gets the simplified Facebook Messenger schema focused on basic outbound messaging.
    /// </summary>
        public static ChannelSchema SimpleMessenger => FacebookSchemaBuilder.CreateSimpleMessenger(FacebookConnectorConstants.ConnectorSchemaVersion);

    /// <summary>
    /// Gets the Facebook Messenger schema optimized for notification scenarios.
    /// </summary>
        public static ChannelSchema NotificationMessenger => FacebookSchemaBuilder.CreateNotificationMessenger(FacebookConnectorConstants.ConnectorSchemaVersion);

    /// <summary>
    /// Gets the Facebook Messenger schema with media-oriented capabilities.
    /// </summary>
        public static ChannelSchema MediaMessenger => FacebookSchemaBuilder.CreateMediaMessenger(FacebookConnectorConstants.ConnectorSchemaVersion);
    }

    /// <summary>
    /// Provides Facebook schemas aligned with Graph API v20.0.
    /// </summary>
    public static class FacebookChannelSchemasV20
    {
    /// <summary>
    /// Gets the full Facebook Messenger schema for Graph API v20.0.
    /// </summary>
        public static ChannelSchema FacebookMessenger => FacebookSchemaBuilder.CreateFacebookMessenger(FacebookConnectorConstants.GraphApiVersion20);

    /// <summary>
    /// Gets the simplified Facebook Messenger schema for Graph API v20.0.
    /// </summary>
        public static ChannelSchema SimpleMessenger => FacebookSchemaBuilder.CreateSimpleMessenger(FacebookConnectorConstants.GraphApiVersion20);

    /// <summary>
    /// Gets the notification-focused Facebook Messenger schema for Graph API v20.0.
    /// </summary>
        public static ChannelSchema NotificationMessenger => FacebookSchemaBuilder.CreateNotificationMessenger(FacebookConnectorConstants.GraphApiVersion20);

    /// <summary>
    /// Gets the media-capable Facebook Messenger schema for Graph API v20.0.
    /// </summary>
        public static ChannelSchema MediaMessenger => FacebookSchemaBuilder.CreateMediaMessenger(FacebookConnectorConstants.GraphApiVersion20);
    }

    /// <summary>
    /// Provides Facebook schemas aligned with Graph API v19.0.
    /// </summary>
    public static class FacebookChannelSchemasV19
    {
    /// <summary>
    /// Gets the full Facebook Messenger schema for Graph API v19.0.
    /// </summary>
        public static ChannelSchema FacebookMessenger => FacebookSchemaBuilder.CreateFacebookMessenger(FacebookConnectorConstants.GraphApiVersion19);

    /// <summary>
    /// Gets the simplified Facebook Messenger schema for Graph API v19.0.
    /// </summary>
        public static ChannelSchema SimpleMessenger => FacebookSchemaBuilder.CreateSimpleMessenger(FacebookConnectorConstants.GraphApiVersion19);

    /// <summary>
    /// Gets the notification-focused Facebook Messenger schema for Graph API v19.0.
    /// </summary>
        public static ChannelSchema NotificationMessenger => FacebookSchemaBuilder.CreateNotificationMessenger(FacebookConnectorConstants.GraphApiVersion19);

    /// <summary>
    /// Gets the media-capable Facebook Messenger schema for Graph API v19.0.
    /// </summary>
        public static ChannelSchema MediaMessenger => FacebookSchemaBuilder.CreateMediaMessenger(FacebookConnectorConstants.GraphApiVersion19);
    }

    /// <summary>
    /// Provides Facebook schemas aligned with Graph API v18.0.
    /// </summary>
    public static class FacebookChannelSchemasV18
    {
    /// <summary>
    /// Gets the full Facebook Messenger schema for Graph API v18.0.
    /// </summary>
        public static ChannelSchema FacebookMessenger => FacebookSchemaBuilder.CreateFacebookMessenger(FacebookConnectorConstants.GraphApiVersion18);

    /// <summary>
    /// Gets the simplified Facebook Messenger schema for Graph API v18.0.
    /// </summary>
        public static ChannelSchema SimpleMessenger => FacebookSchemaBuilder.CreateSimpleMessenger(FacebookConnectorConstants.GraphApiVersion18);

    /// <summary>
    /// Gets the notification-focused Facebook Messenger schema for Graph API v18.0.
    /// </summary>
        public static ChannelSchema NotificationMessenger => FacebookSchemaBuilder.CreateNotificationMessenger(FacebookConnectorConstants.GraphApiVersion18);

    /// <summary>
    /// Gets the media-capable Facebook Messenger schema for Graph API v18.0.
    /// </summary>
        public static ChannelSchema MediaMessenger => FacebookSchemaBuilder.CreateMediaMessenger(FacebookConnectorConstants.GraphApiVersion18);
    }

    internal static class FacebookSchemaBuilder
    {
        internal static ChannelSchema CreateFacebookMessenger(string graphApiVersion) => CreateBaseSchema(NormalizeSupportedVersion(graphApiVersion));

        internal static ChannelSchema CreateSimpleMessenger(string graphApiVersion) => ChannelSchemaBuilder.From(CreateFacebookMessenger(graphApiVersion), "Facebook Simple Messenger")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .RemoveParameter(FacebookConnectionParameters.WebhookUrl)
            .RemoveParameter(FacebookConnectionParameters.VerifyToken)
            .RemoveContentType(MessageContentType.Media)
            .RemoveMessageProperty("QuickReplies")
            .RemoveMessageProperty("Tag")
            .Build();

        internal static ChannelSchema CreateNotificationMessenger(string graphApiVersion) => ChannelSchemaBuilder.From(CreateFacebookMessenger(graphApiVersion), "Facebook Notification Messenger")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .RemoveParameter(FacebookConnectionParameters.WebhookUrl)
            .RemoveParameter(FacebookConnectionParameters.VerifyToken)
            .RemoveMessageProperty("QuickReplies")
            .Build();

        internal static ChannelSchema CreateMediaMessenger(string graphApiVersion) => ChannelSchemaBuilder.From(CreateFacebookMessenger(graphApiVersion), "Facebook Media Messenger")
            .RemoveCapability(ChannelCapability.ReceiveMessages)
            .AddMessageProperty("Attachment", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "JSON object defining attachment (image, audio, video, file)";
            })
            .AddMessageProperty("Template", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "JSON object defining structured message template";
            })
            .Build();

        private static ChannelSchema CreateBaseSchema(string version) => new ChannelSchemaBuilder(
                FacebookConnectorConstants.Provider,
                FacebookConnectorConstants.MessengerChannel,
                version)
            .WithDisplayName("Facebook Messenger Connector")
            .WithCapabilities(
                ChannelCapability.SendMessages |
                ChannelCapability.ReceiveMessages |
                ChannelCapability.MediaAttachments |
                ChannelCapability.HealthCheck |
                ChannelCapability.InteractiveContent)
            .AddParameter(new ChannelParameter(FacebookConnectionParameters.PageAccessToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Facebook Page Access Token - obtained from Facebook App settings"
            })
            .AddParameter(new ChannelParameter(FacebookConnectionParameters.PageId, DataType.String)
            {
                IsRequired = true,
                Description = "Facebook Page ID - the ID of the Facebook Page to send messages from"
            })
            .AddParameter(new ChannelParameter(FacebookConnectionParameters.WebhookUrl, DataType.String)
            {
                IsRequired = false,
                Description = "URL to receive webhook notifications for incoming messages"
            })
            .AddParameter(new ChannelParameter(FacebookConnectionParameters.VerifyToken, DataType.String)
            {
                IsRequired = false,
                IsSensitive = true,
                Description = "Webhook verification token for Facebook webhook validation"
            })
            .AddContentType(MessageContentType.PlainText)
            .AddContentType(MessageContentType.Media)
            .AddContentType(MessageContentType.Button)
            .AddContentType(MessageContentType.QuickReply)
            .AddContentType(MessageContentType.Carousel)
            .AddContentType(MessageContentType.ListPicker)
            .HandlesMessageEndpoint(EndpointType.UserId, e =>
            {
                e.CanSend = true;
                e.CanReceive = true;
                e.IsRequired = true;
            })
            .HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
            {
                e.CanSend = true;
                e.CanReceive = false;
                e.IsRequired = false;
            })
            .HandlesMessageEndpoint(EndpointType.Url, e =>
            {
                e.CanSend = false;
                e.CanReceive = true;
            })
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Page Access Token")
                .WithField(FacebookConnectionParameters.PageAccessToken, DataType.String, f =>
                {
                    f.DisplayName = "Page Access Token";
                    f.Description = "Facebook Page Access Token obtained from Facebook App settings";
                    f.AuthenticationRole = "principal";
                    f.IsSensitive = true;
                }))
            .AddMessageProperty("QuickReplies", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "JSON array of quick reply options for the message";
            })
            .AddMessageProperty("NotificationType", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Push notification type: REGULAR, SILENT_PUSH, or NO_PUSH";
            })
            .AddMessageProperty("MessagingType", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Message type: RESPONSE, UPDATE, MESSAGE_TAG, or NON_PROMOTIONAL_SUBSCRIPTION";
            })
            .AddMessageProperty("Tag", DataType.String, p =>
            {
                p.IsRequired = false;
                p.Description = "Message tag for sending outside 24-hour window";
            })
            .Build();

        private static string NormalizeSupportedVersion(string graphApiVersion)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(graphApiVersion, nameof(graphApiVersion));

            var matchingVersion = FacebookConnectorConstants.SupportedSchemaVersions
                .FirstOrDefault(version => string.Equals(version, graphApiVersion, StringComparison.OrdinalIgnoreCase));

            return matchingVersion ?? throw new ArgumentException(
                $"Unsupported Facebook Graph API schema version '{graphApiVersion}'. Supported versions: {string.Join(", ", FacebookConnectorConstants.SupportedSchemaVersions)}",
                nameof(graphApiVersion));
        }
    }
}
