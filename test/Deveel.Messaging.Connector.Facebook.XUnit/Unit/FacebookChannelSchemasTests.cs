//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Unit tests for Facebook channel schemas.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FacebookChannelSchemas")]
public class FacebookChannelSchemasTests
{
    public static IEnumerable<object[]> VersionedSchemaGroups =>
        new[]
        {
            new object[]
            {
                FacebookConnectorConstants.GraphApiVersion20,
                FacebookChannelSchemasV20.FacebookMessenger,
                FacebookChannelSchemasV20.SimpleMessenger,
                FacebookChannelSchemasV20.NotificationMessenger,
                FacebookChannelSchemasV20.MediaMessenger
            },
            new object[]
            {
                FacebookConnectorConstants.GraphApiVersion19,
                FacebookChannelSchemasV19.FacebookMessenger,
                FacebookChannelSchemasV19.SimpleMessenger,
                FacebookChannelSchemasV19.NotificationMessenger,
                FacebookChannelSchemasV19.MediaMessenger
            },
            new object[]
            {
                FacebookConnectorConstants.GraphApiVersion18,
                FacebookChannelSchemasV18.FacebookMessenger,
                FacebookChannelSchemasV18.SimpleMessenger,
                FacebookChannelSchemasV18.NotificationMessenger,
                FacebookChannelSchemasV18.MediaMessenger
            }
        };

    [Fact]
    public void Should_HaveCorrectBasicProperties_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        Assert.Equal(FacebookConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(FacebookConnectorConstants.MessengerChannel, schema.ChannelType);
        Assert.Equal(FacebookConnectorConstants.ConnectorSchemaVersion, schema.Version);
        Assert.Equal("Facebook Messenger Connector", schema.DisplayName);
    }

    [Theory]
    [MemberData(nameof(VersionedSchemaGroups))]
    public void Should_PreserveExpectedVersionAcrossSchemaGroup_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema messengerSchema,
        ChannelSchema simpleSchema,
        ChannelSchema notificationSchema,
        ChannelSchema mediaSchema)
    {
        // Arrange
        var schemas = new[] { messengerSchema, simpleSchema, notificationSchema, mediaSchema };

        // Act
        var versions = schemas.Select(x => x.Version).ToList();

        // Assert
        Assert.All(versions, version => Assert.Equal(expectedVersion, version));
        Assert.All(schemas, schema =>
        {
            Assert.Equal(FacebookConnectorConstants.Provider, schema.ChannelProvider);
            Assert.Equal(FacebookConnectorConstants.MessengerChannel, schema.ChannelType);
        });
    }

    [Theory]
    [MemberData(nameof(VersionedSchemaGroups))]
    public void Should_RetainBaseMessengerConfiguration_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema messengerSchema,
        ChannelSchema _,
        ChannelSchema __,
        ChannelSchema ___)
    {
        // Arrange
        _ = __;
        _ = ___;
        var parameters = messengerSchema.Parameters.ToList();
        var contentTypes = messengerSchema.ContentTypes.ToList();

        // Act
        var pageAccessToken = parameters.SingleOrDefault(x => x.Name == "PageAccessToken");
        var pageId = parameters.SingleOrDefault(x => x.Name == "PageId");

        // Assert
        Assert.Equal(expectedVersion, messengerSchema.Version);
        Assert.Equal("Facebook Messenger Connector", messengerSchema.DisplayName);
        Assert.True(messengerSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.Contains(MessageContentType.PlainText, contentTypes);
        Assert.Contains(MessageContentType.Media, contentTypes);
        Assert.NotNull(pageAccessToken);
        Assert.True(pageAccessToken.IsRequired);
        Assert.True(pageAccessToken.IsSensitive);
        Assert.NotNull(pageId);
        Assert.True(pageId.IsRequired);
    }

    [Theory]
    [MemberData(nameof(VersionedSchemaGroups))]
    public void Should_PreserveDerivedSchemaBehavior_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema _,
        ChannelSchema simpleSchema,
        ChannelSchema notificationSchema,
        ChannelSchema mediaSchema)
    {
        // Arrange
        var simpleProperties = simpleSchema.MessageProperties.ToList();
        var notificationProperties = notificationSchema.MessageProperties.ToList();
        var mediaProperties = mediaSchema.MessageProperties.ToList();

        // Act
        var mediaAttachment = mediaProperties.SingleOrDefault(x => x.Name == "Attachment");
        var mediaTemplate = mediaProperties.SingleOrDefault(x => x.Name == "Template");

        // Assert
        Assert.Equal(expectedVersion, simpleSchema.Version);
        Assert.Equal(expectedVersion, notificationSchema.Version);
        Assert.Equal(expectedVersion, mediaSchema.Version);

        Assert.False(simpleSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.DoesNotContain(simpleSchema.Parameters, x => x.Name == "WebhookUrl");
        Assert.DoesNotContain(simpleProperties, x => x.Name == "QuickReplies");

        Assert.False(notificationSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.DoesNotContain(notificationProperties, x => x.Name == "QuickReplies");

        Assert.False(mediaSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.NotNull(mediaAttachment);
        Assert.Equal(DataType.String, mediaAttachment.DataType);
        Assert.NotNull(mediaTemplate);
        Assert.Equal(DataType.String, mediaTemplate.DataType);
    }

    [Fact]
    public void Should_HaveCorrectCapabilities_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var expectedCapabilities = ChannelCapability.SendMessages | 
                                  ChannelCapability.ReceiveMessages |
                                  ChannelCapability.MediaAttachments |
                                  ChannelCapability.HealthCheck;

        Assert.Equal(expectedCapabilities, schema.Capabilities);
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
    }

    [Fact]
    public void Should_HaveRequiredParameters_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var parameters = schema.Parameters.ToList();
        
        var pageAccessTokenParam = parameters.FirstOrDefault(p => p.Name == "PageAccessToken");
        Assert.NotNull(pageAccessTokenParam);
        Assert.True(pageAccessTokenParam.IsRequired);
        Assert.True(pageAccessTokenParam.IsSensitive);
        Assert.Equal(DataType.String, pageAccessTokenParam.DataType);

        var pageIdParam = parameters.FirstOrDefault(p => p.Name == "PageId");
        Assert.NotNull(pageIdParam);
        Assert.True(pageIdParam.IsRequired);
        Assert.Equal(DataType.String, pageIdParam.DataType);
    }

    [Fact]
    public void Should_HaveOptionalParameters_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var parameters = schema.Parameters.ToList();
        
        var webhookUrlParam = parameters.FirstOrDefault(p => p.Name == "WebhookUrl");
        Assert.NotNull(webhookUrlParam);
        Assert.False(webhookUrlParam.IsRequired);

        var verifyTokenParam = parameters.FirstOrDefault(p => p.Name == "VerifyToken");
        Assert.NotNull(verifyTokenParam);
        Assert.False(verifyTokenParam.IsRequired);
        Assert.True(verifyTokenParam.IsSensitive);
    }

    [Fact]
    public void Should_HaveCorrectContentTypes_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var contentTypes = schema.ContentTypes.ToList();
        Assert.Contains(MessageContentType.PlainText, contentTypes);
        Assert.Contains(MessageContentType.Media, contentTypes);
        Assert.Equal(2, contentTypes.Count);
    }

    [Fact]
    public void Should_HaveCorrectEndpoints_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var endpoints = schema.Endpoints.ToList();
        
        var userIdEndpoint = endpoints.FirstOrDefault(e => e.Type == EndpointType.UserId);
        Assert.NotNull(userIdEndpoint);
        Assert.True(userIdEndpoint.CanSend);
        Assert.True(userIdEndpoint.CanReceive);
        Assert.True(userIdEndpoint.IsRequired);

        var urlEndpoint = endpoints.FirstOrDefault(e => e.Type == EndpointType.Url);
        Assert.NotNull(urlEndpoint);
        Assert.False(urlEndpoint.CanSend);
        Assert.True(urlEndpoint.CanReceive);
    }

    [Fact]
    public void Should_HaveCorrectMessageProperties_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var messageProperties = schema.MessageProperties.ToList();
        
        var quickRepliesProperty = messageProperties.FirstOrDefault(p => p.Name == "QuickReplies");
        Assert.NotNull(quickRepliesProperty);
        Assert.False(quickRepliesProperty.IsRequired);
        Assert.Equal(DataType.String, quickRepliesProperty.DataType);

        var notificationTypeProperty = messageProperties.FirstOrDefault(p => p.Name == "NotificationType");
        Assert.NotNull(notificationTypeProperty);
        Assert.False(notificationTypeProperty.IsRequired);

        var messagingTypeProperty = messageProperties.FirstOrDefault(p => p.Name == "MessagingType");
        Assert.NotNull(messagingTypeProperty);
        Assert.False(messagingTypeProperty.IsRequired);

        var tagProperty = messageProperties.FirstOrDefault(p => p.Name == "Tag");
        Assert.NotNull(tagProperty);
        Assert.False(tagProperty.IsRequired);
    }

    [Fact]
    public void Should_HaveCorrectAuthenticationTypes_When_FacebookMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.FacebookMessenger;

        // Assert
        var authTypes = schema.AuthenticationTypes.ToList();
        Assert.Contains(AuthenticationType.Token, authTypes);
        Assert.Single(authTypes);
    }

    [Fact]
    public void Should_BeCorrectDerivation_When_SimpleMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.SimpleMessenger;

        // Assert
        Assert.Equal("Facebook Simple Messenger", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        
        var parameters = schema.Parameters.ToList();
        Assert.DoesNotContain(parameters, p => p.Name == "WebhookUrl");
        Assert.DoesNotContain(parameters, p => p.Name == "VerifyToken");
        
        var contentTypes = schema.ContentTypes.ToList();
        Assert.DoesNotContain(contentTypes, ct => ct == MessageContentType.Media);
        
        var messageProperties = schema.MessageProperties.ToList();
        Assert.DoesNotContain(messageProperties, p => p.Name == "QuickReplies");
        Assert.DoesNotContain(messageProperties, p => p.Name == "Tag");
    }

    [Fact]
    public void Should_BeCorrectDerivation_When_NotificationMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.NotificationMessenger;

        // Assert
        Assert.Equal("Facebook Notification Messenger", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        
        var parameters = schema.Parameters.ToList();
        Assert.DoesNotContain(parameters, p => p.Name == "WebhookUrl");
        Assert.DoesNotContain(parameters, p => p.Name == "VerifyToken");
        
        var messageProperties = schema.MessageProperties.ToList();
        Assert.DoesNotContain(messageProperties, p => p.Name == "QuickReplies");
    }

    [Fact]
    public void Should_BeCorrectDerivation_When_MediaMessengerSchema()
    {
        // Act
        var schema = FacebookChannelSchemas.MediaMessenger;

        // Assert
        Assert.Equal("Facebook Media Messenger", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        
        var messageProperties = schema.MessageProperties.ToList();
        
        var attachmentProperty = messageProperties.FirstOrDefault(p => p.Name == "Attachment");
        Assert.NotNull(attachmentProperty);
        Assert.False(attachmentProperty.IsRequired);
        Assert.Equal(DataType.String, attachmentProperty.DataType);

        var templateProperty = messageProperties.FirstOrDefault(p => p.Name == "Template");
        Assert.NotNull(templateProperty);
        Assert.False(templateProperty.IsRequired);
        Assert.Equal(DataType.String, templateProperty.DataType);
    }

    [Fact]
    public void Should_HaveConsistentProviderAndVersion_When_AllSchemasIsInvoked()
    {
        // Act
        var schemas = new[]
        {
            FacebookChannelSchemas.FacebookMessenger,
            FacebookChannelSchemasV20.FacebookMessenger,
            FacebookChannelSchemasV19.FacebookMessenger,
            FacebookChannelSchemasV18.FacebookMessenger,
            FacebookChannelSchemas.SimpleMessenger,
            FacebookChannelSchemasV20.SimpleMessenger,
            FacebookChannelSchemasV19.SimpleMessenger,
            FacebookChannelSchemasV18.SimpleMessenger,
            FacebookChannelSchemas.NotificationMessenger,
            FacebookChannelSchemasV20.NotificationMessenger,
            FacebookChannelSchemasV19.NotificationMessenger,
            FacebookChannelSchemasV18.NotificationMessenger,
            FacebookChannelSchemas.MediaMessenger,
            FacebookChannelSchemasV20.MediaMessenger,
            FacebookChannelSchemasV19.MediaMessenger,
            FacebookChannelSchemasV18.MediaMessenger
        };

        // Assert
        var expectedVersions = FacebookConnectorConstants.SupportedSchemaVersions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var schema in schemas)
        {
            Assert.Equal(FacebookConnectorConstants.Provider, schema.ChannelProvider);
            Assert.Contains(schema.Version, expectedVersions);
        }
    }

    [Fact]
    public void Should_CanBeValidated_When_AllSchemasIsInvoked()
    {
        // Act
        // Assert
        var schemas = new[]
        {
            FacebookChannelSchemas.FacebookMessenger,
            FacebookChannelSchemasV20.FacebookMessenger,
            FacebookChannelSchemasV19.FacebookMessenger,
            FacebookChannelSchemasV18.FacebookMessenger,
            FacebookChannelSchemas.SimpleMessenger,
            FacebookChannelSchemasV20.SimpleMessenger,
            FacebookChannelSchemasV19.SimpleMessenger,
            FacebookChannelSchemasV18.SimpleMessenger,
            FacebookChannelSchemas.NotificationMessenger,
            FacebookChannelSchemasV20.NotificationMessenger,
            FacebookChannelSchemasV19.NotificationMessenger,
            FacebookChannelSchemasV18.NotificationMessenger,
            FacebookChannelSchemas.MediaMessenger,
            FacebookChannelSchemasV20.MediaMessenger,
            FacebookChannelSchemasV19.MediaMessenger,
            FacebookChannelSchemasV18.MediaMessenger
        };

        foreach (var schema in schemas)
        {
            // Basic validation - should not throw
            Assert.NotNull(schema.ChannelProvider);
            Assert.NotNull(schema.ChannelType);
            Assert.NotNull(schema.Version);
            Assert.NotEmpty(schema.Parameters);
            Assert.NotEmpty(schema.ContentTypes);
            Assert.NotEmpty(schema.Endpoints);
        }
    }

}
