//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Tests for Telegram channel schemas.
	/// </summary>
	public class TelegramChannelSchemasTests
	{
		[Fact]
		public void TelegramBot_Schema_ShouldHaveCorrectConfiguration()
		{
			// Act
			var schema = TelegramChannelSchemas.TelegramBot;

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal(TelegramConnectorConstants.BotChannel, schema.ChannelType);
			Assert.Equal("1.0.0", schema.Version);
			Assert.Equal("Telegram Bot API", schema.DisplayName);

			// Check capabilities
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));

			// Check required parameters
			var botTokenParam = schema.Parameters.FirstOrDefault(p => p.Name == "BotToken");
			Assert.NotNull(botTokenParam);
			Assert.True(botTokenParam.IsRequired);
			Assert.True(botTokenParam.IsSensitive);

			// Check supported content types
			Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
			Assert.Contains(MessageContentType.Media, schema.ContentTypes);
			Assert.Contains(MessageContentType.Location, schema.ContentTypes);
			Assert.Contains(MessageContentType.Json, schema.ContentTypes);

			// Check endpoint configuration
			var endpoints = schema.Endpoints.ToList();
			Assert.Single(endpoints);
			Assert.Equal(EndpointType.Id, endpoints[0].Type);
			Assert.True(endpoints[0].CanSend);
			Assert.True(endpoints[0].CanReceive);
		}

		[Fact]
		public void SimpleTelegramBot_Schema_ShouldHaveBasicConfiguration()
		{
			// Act
			var schema = TelegramChannelSchemas.SimpleTelegramBot;

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("simple-bot", schema.ChannelType);
			Assert.Equal("Simple Telegram Bot", schema.DisplayName);

			// Check capabilities - should only have basic ones
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
			Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
			Assert.False(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));

			// Check content types - should only support text
			Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
			Assert.DoesNotContain(MessageContentType.Media, schema.ContentTypes);
			Assert.DoesNotContain(MessageContentType.Location, schema.ContentTypes);

			// Check endpoint configuration
			var endpoints = schema.Endpoints.ToList();
			Assert.Single(endpoints);
			Assert.Equal(EndpointType.Id, endpoints[0].Type);
			Assert.True(endpoints[0].CanSend);
			Assert.False(endpoints[0].CanReceive);
		}

		[Fact]
		public void NotificationBot_Schema_ShouldHaveNotificationConfiguration()
		{
			// Act
			var schema = TelegramChannelSchemas.NotificationBot;

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("notification-bot", schema.ChannelType);
			Assert.Equal("Telegram Notification Bot", schema.DisplayName);

			// Check capabilities - should only have send capabilities
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
			Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));

			// Check optional parameters
			var defaultChatIdParam = schema.Parameters.FirstOrDefault(p => p.Name == "DefaultChatId");
			Assert.NotNull(defaultChatIdParam);
			Assert.False(defaultChatIdParam.IsRequired);

			var parseModeParam = schema.Parameters.FirstOrDefault(p => p.Name == "ParseMode");
			Assert.NotNull(parseModeParam);
			Assert.Equal("HTML", parseModeParam.DefaultValue);
		}

		[Fact]
		public void WebhookBot_Schema_ShouldHaveWebhookConfiguration()
		{
			// Act
			var schema = TelegramChannelSchemas.WebhookBot;

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("webhook-bot", schema.ChannelType);
			Assert.Equal("Telegram Webhook Bot", schema.DisplayName);

			// Check capabilities
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
			Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));

			// Check required webhook parameters
			var webhookUrlParam = schema.Parameters.FirstOrDefault(p => p.Name == "WebhookUrl");
			Assert.NotNull(webhookUrlParam);
			Assert.True(webhookUrlParam.IsRequired);

			var secretTokenParam = schema.Parameters.FirstOrDefault(p => p.Name == "SecretToken");
			Assert.NotNull(secretTokenParam);
			Assert.True(secretTokenParam.IsRequired);
			Assert.True(secretTokenParam.IsSensitive);

			// Check optional webhook parameters
			var maxConnectionsParam = schema.Parameters.FirstOrDefault(p => p.Name == "MaxConnections");
			Assert.NotNull(maxConnectionsParam);
			Assert.Equal(40, maxConnectionsParam.DefaultValue);
		}

		[Theory]
		[InlineData("TelegramBot")]
		[InlineData("SimpleTelegramBot")]
		[InlineData("NotificationBot")]
		[InlineData("WebhookBot")]
		public void AllSchemas_ShouldHaveBotTokenParameter(string schemaPropertyName)
		{
			// Arrange
			var property = typeof(TelegramChannelSchemas).GetProperty(schemaPropertyName);
			Assert.NotNull(property);

			// Act
			var schema = (IChannelSchema)property.GetValue(null)!;

			// Assert
			var botTokenParam = schema.Parameters.FirstOrDefault(p => p.Name == "BotToken");
			Assert.NotNull(botTokenParam);
			Assert.True(botTokenParam.IsRequired);
			Assert.True(botTokenParam.IsSensitive);
			Assert.Equal(DataType.String, botTokenParam.DataType);
		}

		[Theory]
		[InlineData("TelegramBot")]
		[InlineData("SimpleTelegramBot")]
		[InlineData("NotificationBot")]
		[InlineData("WebhookBot")]
		public void AllSchemas_ShouldSupportIdEndpoints(string schemaPropertyName)
		{
			// Arrange
			var property = typeof(TelegramChannelSchemas).GetProperty(schemaPropertyName);
			Assert.NotNull(property);

			// Act
			var schema = (IChannelSchema)property.GetValue(null)!;

			// Assert
			var endpoints = schema.Endpoints.ToList();
			Assert.Single(endpoints);
			Assert.Equal(EndpointType.Id, endpoints[0].Type);
			Assert.True(endpoints[0].IsRequired);
		}

		[Fact]
		public void TelegramBotSchemaFactory_CreateSchema_ShouldReturnFullSchema()
		{
			// Act
			var schema = TelegramBotSchemaFactory.CreateSchema();

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal(TelegramConnectorConstants.BotChannel, schema.ChannelType);
			Assert.Equal("1.0.0", schema.Version);
			Assert.Equal("Telegram Bot API", schema.DisplayName);
		}

		[Fact]
		public void TelegramBotSchemaFactory_CreateSimpleSchema_ShouldReturnSimpleSchema()
		{
			// Act
			var schema = TelegramBotSchemaFactory.CreateSimpleSchema();

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("simple-bot", schema.ChannelType);
			Assert.Equal("Simple Telegram Bot", schema.DisplayName);
		}

		[Fact]
		public void TelegramBotSchemaFactory_CreateNotificationSchema_ShouldReturnNotificationSchema()
		{
			// Act
			var schema = TelegramBotSchemaFactory.CreateNotificationSchema();

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("notification-bot", schema.ChannelType);
			Assert.Equal("Telegram Notification Bot", schema.DisplayName);
		}

		[Fact]
		public void TelegramBotSchemaFactory_CreateWebhookSchema_ShouldReturnWebhookSchema()
		{
			// Act
			var schema = TelegramBotSchemaFactory.CreateWebhookSchema();

			// Assert
			Assert.NotNull(schema);
			Assert.Equal(TelegramConnectorConstants.Provider, schema.ChannelProvider);
			Assert.Equal("webhook-bot", schema.ChannelType);
			Assert.Equal("Telegram Webhook Bot", schema.DisplayName);
		}
	}
}