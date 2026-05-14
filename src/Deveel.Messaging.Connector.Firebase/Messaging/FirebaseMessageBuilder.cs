//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

using FirebaseAdmin.Messaging;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides static methods for building <see cref="FirebaseAdmin.Messaging.Message"/>
    /// objects from <see cref="IMessage"/> instances.
    /// </summary>
    internal static class FirebaseMessageBuilder
    {
        /// <summary>
        /// Builds a Firebase <see cref="FirebaseAdmin.Messaging.Message"/> from the given
        /// framework <see cref="IMessage"/>.
        /// </summary>
        public static Task<FirebaseAdmin.Messaging.Message> BuildFirebaseMessageAsync(IMessage message)
        {
            var firebaseMessage = new FirebaseAdmin.Messaging.Message();

            // Set target (device token or topic)
            if (message.Receiver?.Type == EndpointType.DeviceId)
            {
                firebaseMessage.Token = message.Receiver.Address;
            }
            else if (message.Receiver?.Type == EndpointType.Topic)
            {
                firebaseMessage.Topic = message.Receiver.Address;
            }
            else
            {
                throw new ArgumentException("Message receiver must be a DeviceId or Topic endpoint");
            }

            var notification = BuildNotification(message);
            if (notification != null)
                firebaseMessage.Notification = notification;

            var data = BuildDataPayload(message);
            if (data.Count > 0)
                firebaseMessage.Data = data;

            var androidConfig = BuildAndroidConfig(message);
            if (androidConfig != null)
                firebaseMessage.Android = androidConfig;

            var apnsConfig = BuildApnsConfig(message);
            if (apnsConfig != null)
                firebaseMessage.Apns = apnsConfig;

            var webPushConfig = BuildWebPushConfig(message);
            if (webPushConfig != null)
                firebaseMessage.Webpush = webPushConfig;

            return Task.FromResult(firebaseMessage);
        }

        /// <summary>
        /// Builds the notification payload from message content and properties.
        /// </summary>
        public static Notification? BuildNotification(IMessage message)
        {
            var title = GetMessageProperty(message, "Title");
            var body = (message.Content as ITextContent)?.Text ??
                      GetMessageProperty(message, "Body");
            var imageUrl = GetMessageProperty(message, "ImageUrl");

            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body))
                return null;

            var notification = new Notification
            {
                Title = title,
                Body = body
            };

            if (!string.IsNullOrEmpty(imageUrl))
                notification.ImageUrl = imageUrl;

            return notification;
        }

        /// <summary>
        /// Builds the data payload from message content and custom data.
        /// </summary>
        public static Dictionary<string, string> BuildDataPayload(IMessage message)
        {
            var data = new Dictionary<string, string>();

            var customData = GetMessageProperty(message, "CustomData");
            if (!string.IsNullOrEmpty(customData))
            {
                try
                {
                    using var document = JsonDocument.Parse(customData);
                    foreach (var property in document.RootElement.EnumerateObject())
                        data[property.Name] = property.Value.ToString();
                }
                catch (JsonException)
                {
                    // Invalid JSON, add as single field
                    data["customData"] = customData;
                }
            }

            if (!string.IsNullOrEmpty(message.Id))
                data["messageId"] = message.Id;

            return data;
        }

        /// <summary>
        /// Builds Android-specific configuration from message properties.
        /// </summary>
        public static AndroidConfig? BuildAndroidConfig(IMessage message)
        {
            var hasConfig = false;
            var androidConfig = new AndroidConfig();
            var androidNotification = new AndroidNotification();

            var color = GetMessageProperty(message, "Color");
            if (!string.IsNullOrEmpty(color)) { androidNotification.Color = color; hasConfig = true; }

            var sound = GetMessageProperty(message, "Sound");
            if (!string.IsNullOrEmpty(sound)) { androidNotification.Sound = sound; hasConfig = true; }

            var tag = GetMessageProperty(message, "Tag");
            if (!string.IsNullOrEmpty(tag)) { androidNotification.Tag = tag; hasConfig = true; }

            var clickAction = GetMessageProperty(message, "ClickAction");
            if (!string.IsNullOrEmpty(clickAction)) { androidNotification.ClickAction = clickAction; hasConfig = true; }

            if (hasConfig)
                androidConfig.Notification = androidNotification;

            var priority = GetMessageProperty(message, "Priority");
            if (!string.IsNullOrEmpty(priority))
            {
                androidConfig.Priority = priority.Equals("high", StringComparison.OrdinalIgnoreCase) ? Priority.High : Priority.Normal;
                hasConfig = true;
            }

            var timeToLiveStr = GetMessageProperty(message, "TimeToLive");
            if (!string.IsNullOrEmpty(timeToLiveStr) && int.TryParse(timeToLiveStr, out var ttlSeconds))
            {
                androidConfig.TimeToLive = TimeSpan.FromSeconds(ttlSeconds);
                hasConfig = true;
            }

            var collapseKey = GetMessageProperty(message, "CollapseKey");
            if (!string.IsNullOrEmpty(collapseKey)) { androidConfig.CollapseKey = collapseKey; hasConfig = true; }

            var restrictedPackageName = GetMessageProperty(message, "RestrictedPackageName");
            if (!string.IsNullOrEmpty(restrictedPackageName)) { androidConfig.RestrictedPackageName = restrictedPackageName; hasConfig = true; }

            return hasConfig ? androidConfig : null;
        }

        /// <summary>
        /// Builds iOS-specific APNS configuration from message properties.
        /// </summary>
        public static ApnsConfig? BuildApnsConfig(IMessage message)
        {
            var hasConfig = false;
            var apnsConfig = new ApnsConfig();
            var apsPayload = new Aps();

            var badgeStr = GetMessageProperty(message, "Badge");
            if (!string.IsNullOrEmpty(badgeStr) && int.TryParse(badgeStr, out var badgeCount))
            {
                apsPayload.Badge = badgeCount;
                hasConfig = true;
            }

            var sound = GetMessageProperty(message, "Sound");
            if (!string.IsNullOrEmpty(sound)) { apsPayload.Sound = sound; hasConfig = true; }

            var contentAvailableStr = GetMessageProperty(message, "ContentAvailable");
            if (!string.IsNullOrEmpty(contentAvailableStr) && bool.TryParse(contentAvailableStr, out var isContentAvailable) && isContentAvailable)
            {
                apsPayload.ContentAvailable = true;
                hasConfig = true;
            }

            var mutableContentStr = GetMessageProperty(message, "MutableContent");
            if (!string.IsNullOrEmpty(mutableContentStr) && bool.TryParse(mutableContentStr, out var isMutableContent) && isMutableContent)
            {
                apsPayload.MutableContent = true;
                hasConfig = true;
            }

            var threadId = GetMessageProperty(message, "ThreadId");
            if (!string.IsNullOrEmpty(threadId)) { apsPayload.ThreadId = threadId; hasConfig = true; }

            if (hasConfig)
                apnsConfig.Aps = apsPayload;

            return hasConfig ? apnsConfig : null;
        }

        /// <summary>
        /// Builds web push configuration from message properties.
        /// </summary>
        public static WebpushConfig? BuildWebPushConfig(IMessage message)
        {
            // Extendable for web-push specific configuration
            return null;
        }

        /// <summary>
        /// Gets a message property value as a string.
        /// </summary>
        public static string? GetMessageProperty(IMessage message, string propertyName)
        {
            if (message.Properties?.TryGetValue(propertyName, out var property) == true)
                return property.Value?.ToString();

            return null;
        }
    }
}

