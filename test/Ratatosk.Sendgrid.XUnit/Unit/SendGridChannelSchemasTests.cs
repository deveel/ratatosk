using Microsoft.Extensions.Logging;

namespace Ratatosk;

/// <summary>
/// Tests for the <see cref="SendGridChannelSchemas"/> class to verify
/// the schema configurations and their derivations.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SendGridChannelSchemas")]
public class SendGridChannelSchemasTests
{
    [Fact]
    public void Should_HaveCorrectBasicProperties_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        Assert.Equal(SendGridConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(SendGridConnectorConstants.EmailChannel, schema.ChannelType);
        Assert.Equal("1.0.0", schema.Version);
        Assert.Equal("SendGrid Email Connector", schema.DisplayName);
    }

    [Fact]
    public void Should_HaveRequiredCapabilities_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
    }

    [Fact]
    public void Should_HaveRequiredParameters_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        var apiKeyParam = schema.Parameters.FirstOrDefault(p => p.Name == "ApiKey");
        Assert.NotNull(apiKeyParam);
        Assert.True(apiKeyParam.IsRequired);
        Assert.True(apiKeyParam.IsSensitive);
        Assert.Equal(DataType.String, apiKeyParam.DataType);

        var sandboxParam = schema.Parameters.FirstOrDefault(p => p.Name == "SandboxMode");
        Assert.NotNull(sandboxParam);
        Assert.False(sandboxParam.IsRequired);
        Assert.Equal(DataType.Boolean, sandboxParam.DataType);
        Assert.Equal(false, sandboxParam.DefaultValue);
    }

    [Fact]
    public void Should_HaveCorrectContentTypes_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
        Assert.Contains(MessageContentType.Html, schema.ContentTypes);
        Assert.Contains(MessageContentType.Template, schema.ContentTypes);
        Assert.Contains(MessageContentType.Multipart, schema.ContentTypes);
    }

    [Fact]
    public void Should_HandleEmailEndpoints_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        var emailEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.EmailAddress);
        Assert.NotNull(emailEndpoint);
        Assert.True(emailEndpoint.CanSend);
        Assert.True(emailEndpoint.CanReceive); // Email addresses can be both senders and receivers
        Assert.True(emailEndpoint.IsRequired);

        var urlEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.Url);
        Assert.NotNull(urlEndpoint);
        Assert.False(urlEndpoint.CanSend);
        Assert.True(urlEndpoint.CanReceive);
    }

    [Fact]
    public void Should_HaveRequiredMessageProperties_When_SendGridEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Assert
        var subjectProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "Subject");
        Assert.NotNull(subjectProperty);
        Assert.True(subjectProperty.IsRequired);
        Assert.Equal(DataType.String, subjectProperty.DataType);

        var priorityProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "Priority");
        Assert.NotNull(priorityProperty);
        Assert.False(priorityProperty.IsRequired);

        var categoriesProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "Categories");
        Assert.NotNull(categoriesProperty);
        Assert.False(categoriesProperty.IsRequired);
    }

    [Fact]
    public void Should_BeCorrectlyDerived_When_SimpleEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.SimpleEmail;

        // Assert
        Assert.Equal("SendGrid Simple Email", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        
        // Should still have basic capabilities
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));

        // Should not have template content type
        Assert.DoesNotContain(MessageContentType.Template, schema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Multipart, schema.ContentTypes);
        
        // Should still have basic content types
        Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
        Assert.Contains(MessageContentType.Html, schema.ContentTypes);
    }

    [Fact]
    public void Should_BeCorrectlyDerived_When_TransactionalEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.TransactionalEmail;

        // Assert
        Assert.Equal("SendGrid Transactional Email", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));

        // Should not have scheduling properties
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "SendAt");
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "BatchId");
    }

    [Fact]
    public void Should_HaveMarketingProperties_When_MarketingEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.MarketingEmail;

        // Assert
        Assert.Equal("SendGrid Marketing Email", schema.DisplayName);
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));

        // Should have marketing-specific properties
        var listIdProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "ListId");
        Assert.NotNull(listIdProperty);
        Assert.False(listIdProperty.IsRequired);

        var campaignIdProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "CampaignId");
        Assert.NotNull(campaignIdProperty);
        Assert.False(campaignIdProperty.IsRequired);
    }

    [Fact]
    public void Should_BeTemplateOptimized_When_TemplateEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.TemplateEmail;

        // Assert
        Assert.Equal("SendGrid Template Email", schema.DisplayName);
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));

        // Should only have template content type
        Assert.Contains(MessageContentType.Template, schema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.PlainText, schema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Html, schema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Multipart, schema.ContentTypes);

        // Should have template-specific properties
        var templateIdProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "TemplateId");
        Assert.NotNull(templateIdProperty);
        Assert.True(templateIdProperty.IsRequired);

        var templateDataProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "TemplateData");
        Assert.NotNull(templateDataProperty);
        Assert.False(templateDataProperty.IsRequired);
    }

    [Fact]
    public void Should_HaveBulkCapabilities_When_BulkEmailIsInvoked()
    {
        // Arrange
        // Act
        var schema = SendGridChannelSchemas.BulkEmail;

        // Assert
        Assert.Equal("SendGrid Bulk Email", schema.DisplayName);
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));

        // Should have bulk-specific properties
        var mailBatchIdProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "MailBatchId");
        Assert.NotNull(mailBatchIdProperty);
        Assert.False(mailBatchIdProperty.IsRequired);

        var unsubscribeGroupIdProperty = schema.MessageProperties.FirstOrDefault(p => p.Name == "UnsubscribeGroupId");
        Assert.NotNull(unsubscribeGroupIdProperty);
        Assert.False(unsubscribeGroupIdProperty.IsRequired);
    }

    [Fact]
    public void Should_SupportEmailAddressEndpoint_When_AllSchemasIsInvoked()
    {
        // Arrange
        var schemas = new[]
        {
            SendGridChannelSchemas.SendGridEmail,
            SendGridChannelSchemas.SimpleEmail,
            SendGridChannelSchemas.TransactionalEmail,
            SendGridChannelSchemas.MarketingEmail,
            SendGridChannelSchemas.TemplateEmail,
            SendGridChannelSchemas.BulkEmail
        };

        // Act
        // Assert
        foreach (var schema in schemas)
        {
            var emailEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.EmailAddress);
            Assert.NotNull(emailEndpoint);
            Assert.True(emailEndpoint.CanSend);
        }
    }

    [Fact]
    public void Should_RequireApiKey_When_AllSchemasIsInvoked()
    {
        // Arrange
        var schemas = new[]
        {
            SendGridChannelSchemas.SendGridEmail,
            SendGridChannelSchemas.SimpleEmail,
            SendGridChannelSchemas.TransactionalEmail,
            SendGridChannelSchemas.MarketingEmail,
            SendGridChannelSchemas.TemplateEmail,
            SendGridChannelSchemas.BulkEmail
        };

        // Act
        // Assert
        foreach (var schema in schemas)
        {
            var apiKeyParam = schema.Parameters.FirstOrDefault(p => p.Name == "ApiKey");
            Assert.NotNull(apiKeyParam);
            Assert.True(apiKeyParam.IsRequired);
            Assert.True(apiKeyParam.IsSensitive);
        }
    }

    [Fact]
    public void Should_SupportBasicSendingCapability_When_AllSchemasIsInvoked()
    {
        // Arrange
        var schemas = new[]
        {
            SendGridChannelSchemas.SendGridEmail,
            SendGridChannelSchemas.SimpleEmail,
            SendGridChannelSchemas.TransactionalEmail,
            SendGridChannelSchemas.MarketingEmail,
            SendGridChannelSchemas.TemplateEmail,
            SendGridChannelSchemas.BulkEmail
        };

        // Act
        // Assert
        foreach (var schema in schemas)
        {
            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        }
    }
}