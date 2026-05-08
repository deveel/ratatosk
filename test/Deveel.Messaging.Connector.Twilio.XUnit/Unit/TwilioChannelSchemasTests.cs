namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="TwilioChannelSchemas"/> class and its versioned variants
/// to verify the predefined Twilio SMS and WhatsApp channel schemas are configured correctly.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioChannelSchemas")]
public class TwilioChannelSchemasTests
{
    #region MemberData Sources

    /// <summary>
    /// Provides versioned SMS schema groups for theory tests.
    /// Each entry contains: expected version, base SMS schema, and available derived SMS schemas.
    /// </summary>
    public static IEnumerable<object[]> VersionedSmsSchemaGroups =>
    [
        [
            TwilioConnectorConstants.SdkVersion6,
            TwilioChannelSchemasV6.TwilioSms,
            new[] { TwilioChannelSchemasV6.SimpleSms, TwilioChannelSchemasV6.NotificationSms, TwilioChannelSchemasV6.BulkSms }
        ],
        [
            TwilioConnectorConstants.SdkVersion5,
            TwilioChannelSchemasV5.TwilioSms,
            new[] { TwilioChannelSchemasV5.SimpleSms, TwilioChannelSchemasV5.NotificationSms }
        ]
    ];

    /// <summary>
    /// Provides versioned WhatsApp schema groups for theory tests.
    /// Each entry contains: expected version, base WhatsApp schema, and available derived WhatsApp schemas.
    /// </summary>
    public static IEnumerable<object[]> VersionedWhatsAppSchemaGroups =>
    [
        [
            TwilioConnectorConstants.SdkVersion6,
            TwilioChannelSchemasV6.TwilioWhatsApp,
            new[] { TwilioChannelSchemasV6.SimpleWhatsApp, TwilioChannelSchemasV6.WhatsAppTemplates }
        ],
        [
            TwilioConnectorConstants.SdkVersion5,
            TwilioChannelSchemasV5.TwilioWhatsApp,
            new[] { TwilioChannelSchemasV5.SimpleWhatsApp }
        ]
    ];

    #endregion

    #region TwilioSms (v7.0 current)

    [Fact]
    public void Should_HaveCorrectBasicProperties_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.SmsChannel, schema.ChannelType);
        Assert.Equal(TwilioConnectorConstants.ConnectorSchemaVersion, schema.Version);
        Assert.Equal("Twilio SMS Connector", schema.DisplayName);
    }

    [Fact]
    public void Should_HaveCorrectBasicProperties_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.WhatsAppChannel, schema.ChannelType);
        Assert.Equal(TwilioConnectorConstants.ConnectorSchemaVersion, schema.Version);
        Assert.Equal("Twilio WhatsApp Business API Connector", schema.DisplayName);
    }

    [Fact]
    public void Should_HaveCorrectCapabilities_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
    }

    [Fact]
    public void Should_HaveCorrectCapabilities_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
    }

    [Fact]
    public void Should_HaveRequiredParameters_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        var requiredParams = schema.Parameters.Where(p => p.IsRequired).ToList();
        Assert.Equal(2, requiredParams.Count);
        Assert.Contains(requiredParams, p => p.Name == "AccountSid" && p.DataType == DataType.String);
        Assert.Contains(requiredParams, p => p.Name == "AuthToken" && p.DataType == DataType.String && p.IsSensitive);
    }

    [Fact]
    public void Should_HaveRequiredParameters_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        var requiredParams = schema.Parameters.Where(p => p.IsRequired).ToList();
        Assert.Equal(2, requiredParams.Count);
        Assert.Contains(requiredParams, p => p.Name == "AccountSid" && p.DataType == DataType.String);
        Assert.Contains(requiredParams, p => p.Name == "AuthToken" && p.DataType == DataType.String && p.IsSensitive);
    }

    [Fact]
    public void Should_HaveOptionalParameters_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        var optionalParams = schema.Parameters.Where(p => !p.IsRequired).ToList();
        Assert.True(optionalParams.Count >= 5);
        Assert.Contains(optionalParams, p => p.Name == "WebhookUrl");
        Assert.Contains(optionalParams, p => p.Name == "StatusCallback");
        Assert.Contains(optionalParams, p => p.Name == "ValidityPeriod" && p.DefaultValue?.Equals(14400) == true);
        Assert.Contains(optionalParams, p => p.Name == "MaxPrice");
        Assert.Contains(optionalParams, p => p.Name == "MessagingServiceSid");
    }

    [Fact]
    public void Should_HaveOptionalParameters_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        var optionalParams = schema.Parameters.Where(p => !p.IsRequired).ToList();
        Assert.True(optionalParams.Count >= 2);
        Assert.Contains(optionalParams, p => p.Name == "WebhookUrl");
        Assert.Contains(optionalParams, p => p.Name == "StatusCallback");
    }

    [Fact]
    public void Should_HaveCorrectContentTypes_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        Assert.Equal(2, schema.ContentTypes.Count);
        Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
        Assert.Contains(MessageContentType.Media, schema.ContentTypes);
    }

    [Fact]
    public void Should_HaveCorrectContentTypes_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.Equal(3, schema.ContentTypes.Count);
        Assert.Contains(MessageContentType.PlainText, schema.ContentTypes);
        Assert.Contains(MessageContentType.Media, schema.ContentTypes);
        Assert.Contains(MessageContentType.Template, schema.ContentTypes);
    }

    [Fact]
    public void Should_HaveCorrectEndpoints_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        Assert.Equal(2, schema.Endpoints.Count);

        var phoneEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.PhoneNumber);
        Assert.NotNull(phoneEndpoint);
        Assert.True(phoneEndpoint.CanSend);
        Assert.True(phoneEndpoint.CanReceive);

        var urlEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.Url);
        Assert.NotNull(urlEndpoint);
        Assert.False(urlEndpoint.CanSend);
        Assert.True(urlEndpoint.CanReceive);
    }

    [Fact]
    public void Should_HaveCorrectEndpoints_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.Equal(2, schema.Endpoints.Count);

        var phoneEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.PhoneNumber);
        Assert.NotNull(phoneEndpoint);
        Assert.True(phoneEndpoint.CanSend);
        Assert.True(phoneEndpoint.CanReceive);

        var urlEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.Url);
        Assert.NotNull(urlEndpoint);
        Assert.False(urlEndpoint.CanSend);
        Assert.True(urlEndpoint.CanReceive);
    }

    [Fact]
    public void Should_HaveCorrectAuthenticationType_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert
        Assert.Single(schema.AuthenticationTypes);
        Assert.Contains(AuthenticationType.Basic, schema.AuthenticationTypes);
    }

    [Fact]
    public void Should_HaveCorrectAuthenticationType_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.Single(schema.AuthenticationTypes);
        Assert.Contains(AuthenticationType.Basic, schema.AuthenticationTypes);
    }

    [Fact]
    public void Should_HaveMessageProperties_When_TwilioSmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioSms;

        // Assert — v7.0 includes AttemptLimits, SmartEncoded, PersistentAction
        Assert.True(schema.MessageProperties.Count >= 6);
        var optionalProps = schema.MessageProperties.Where(p => !p.IsRequired).ToList();
        Assert.Contains(optionalProps, p => p.Name == "ValidityPeriod");
        Assert.Contains(optionalProps, p => p.Name == "MaxPrice");
        Assert.Contains(optionalProps, p => p.Name == "ProvideCallback");
        Assert.Contains(optionalProps, p => p.Name == "AttemptLimits");
        Assert.Contains(optionalProps, p => p.Name == "SmartEncoded");
        Assert.Contains(optionalProps, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_HaveMessageProperties_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert — v7.0 includes PersistentAction
        Assert.True(schema.MessageProperties.Count >= 2);
        var optionalProps = schema.MessageProperties.Where(p => !p.IsRequired).ToList();
        Assert.Contains(optionalProps, p => p.Name == "ProvideCallback");
        Assert.Contains(optionalProps, p => p.Name == "PersistentAction");
    }

    #endregion

    #region Versioned schema groups

    [Theory]
    [MemberData(nameof(VersionedSmsSchemaGroups))]
    public void Should_PreserveExpectedVersionAcrossSmsSchemaGroup_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema baseSchema,
        ChannelSchema[] derivedSchemas)
    {
        // Arrange
        var allSchemas = derivedSchemas.Prepend(baseSchema).ToList();

        // Act
        var versions = allSchemas.Select(s => s.Version).ToList();

        // Assert
        Assert.All(versions, version => Assert.Equal(expectedVersion, version));
    }

    [Theory]
    [MemberData(nameof(VersionedSmsSchemaGroups))]
    public void Should_RetainBaseSmsConfiguration_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema baseSchema,
        ChannelSchema[] _)
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, baseSchema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.SmsChannel, baseSchema.ChannelType);
        Assert.Equal(expectedVersion, baseSchema.Version);
        Assert.Equal("Twilio SMS Connector", baseSchema.DisplayName);
        Assert.True(baseSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(baseSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.Contains(baseSchema.Parameters, p => p.Name == "AccountSid" && p.IsRequired);
        Assert.Contains(baseSchema.Parameters, p => p.Name == "AuthToken" && p.IsRequired && p.IsSensitive);
    }

    [Theory]
    [MemberData(nameof(VersionedWhatsAppSchemaGroups))]
    public void Should_PreserveExpectedVersionAcrossWhatsAppSchemaGroup_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema baseSchema,
        ChannelSchema[] derivedSchemas)
    {
        // Arrange
        var allSchemas = derivedSchemas.Prepend(baseSchema).ToList();

        // Act
        var versions = allSchemas.Select(s => s.Version).ToList();

        // Assert
        Assert.All(versions, version => Assert.Equal(expectedVersion, version));
    }

    [Theory]
    [MemberData(nameof(VersionedWhatsAppSchemaGroups))]
    public void Should_RetainBaseWhatsAppConfiguration_When_VersionedSchemaClassIsUsed(
        string expectedVersion,
        ChannelSchema baseSchema,
        ChannelSchema[] _)
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, baseSchema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.WhatsAppChannel, baseSchema.ChannelType);
        Assert.Equal(expectedVersion, baseSchema.Version);
        Assert.Equal("Twilio WhatsApp Business API Connector", baseSchema.DisplayName);
        Assert.True(baseSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(baseSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.Contains(baseSchema.Parameters, p => p.Name == "AccountSid" && p.IsRequired);
        Assert.Contains(baseSchema.Parameters, p => p.Name == "AuthToken" && p.IsRequired && p.IsSensitive);
    }

    #endregion

    #region SDK version capability differences

    [Fact]
    public void Should_NotHaveSmartEncodedOrAttemptLimitsOrPersistentAction_When_V6SmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV6.TwilioSms;

        // Assert
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "SmartEncoded");
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "AttemptLimits");
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_HaveMessagingServiceSid_When_V6SmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV6.TwilioSms;

        // Assert
        Assert.Contains(schema.Parameters, p => p.Name == "MessagingServiceSid");
    }

    [Fact]
    public void Should_NotHaveMessagingServiceSid_When_V5SmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV5.TwilioSms;

        // Assert
        Assert.DoesNotContain(schema.Parameters, p => p.Name == "MessagingServiceSid");
    }

    [Fact]
    public void Should_NotHaveSmartEncodedOrAttemptLimitsOrPersistentAction_When_V5SmsIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV5.TwilioSms;

        // Assert
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "SmartEncoded");
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "AttemptLimits");
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_HaveTemplateSupportAndPersistentAction_When_V7WhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.Contains(schema.ContentTypes, ct => ct == MessageContentType.Template);
        Assert.Contains(schema.MessageProperties, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_HaveTemplateSupportButNoPersistentAction_When_V6WhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV6.TwilioWhatsApp;

        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.Contains(schema.ContentTypes, ct => ct == MessageContentType.Template);
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_NotHaveTemplateSupportOrPersistentAction_When_V5WhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var schema = TwilioChannelSchemasV5.TwilioWhatsApp;

        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.DoesNotContain(schema.ContentTypes, ct => ct == MessageContentType.Template);
        Assert.DoesNotContain(schema.MessageProperties, p => p.Name == "PersistentAction");
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_BulkSmsBuiltForV5()
    {
        // Arrange
        // Act & Assert — BulkSms requires MessagingServiceSid which is not available in SDK v5.0
        var ex = Assert.Throws<InvalidOperationException>(
            () => TwilioSchemaBuilder.CreateBulkSms(TwilioConnectorConstants.SdkVersion5));

        Assert.Contains("6.0", ex.Message);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_WhatsAppTemplatesBuiltForV5()
    {
        // Arrange
        // Act & Assert — WhatsAppTemplates requires template support which is not available in SDK v5.0
        var ex = Assert.Throws<InvalidOperationException>(
            () => TwilioSchemaBuilder.CreateWhatsAppTemplates(TwilioConnectorConstants.SdkVersion5));

        Assert.Contains("6.0", ex.Message);
    }

    #endregion

    #region Derived schemas (v7.0 current)

    [Fact]
    public void Should_BeCorrectlyDerivedFromTwilioSms_When_SimpleSmsIsInvoked()
    {
        // Arrange
        // Act
        var baseSchema = TwilioChannelSchemas.TwilioSms;
        var simplifiedSchema = TwilioChannelSchemas.SimpleSms;

        // Assert
        Assert.Equal(baseSchema.ChannelProvider, simplifiedSchema.ChannelProvider);
        Assert.Equal(baseSchema.ChannelType, simplifiedSchema.ChannelType);
        Assert.Equal(baseSchema.Version, simplifiedSchema.Version);
        Assert.True(baseSchema.IsCompatibleWith(simplifiedSchema));
        Assert.Equal("Twilio Simple SMS", simplifiedSchema.DisplayName);

        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
        Assert.False(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));

        Assert.DoesNotContain(simplifiedSchema.Parameters, p => p.Name == "WebhookUrl");
        Assert.DoesNotContain(simplifiedSchema.Parameters, p => p.Name == "StatusCallback");
        Assert.DoesNotContain(simplifiedSchema.Parameters, p => p.Name == "MessagingServiceSid");

        Assert.Single(simplifiedSchema.ContentTypes);
        Assert.Contains(MessageContentType.PlainText, simplifiedSchema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Media, simplifiedSchema.ContentTypes);
    }

    [Fact]
    public void Should_BeCorrectlyDerivedFromTwilioWhatsApp_When_SimpleWhatsAppIsInvoked()
    {
        // Arrange
        // Act
        var baseSchema = TwilioChannelSchemas.TwilioWhatsApp;
        var simplifiedSchema = TwilioChannelSchemas.SimpleWhatsApp;

        // Assert
        Assert.Equal(baseSchema.ChannelProvider, simplifiedSchema.ChannelProvider);
        Assert.Equal(baseSchema.ChannelType, simplifiedSchema.ChannelType);
        Assert.Equal(baseSchema.Version, simplifiedSchema.Version);
        Assert.True(baseSchema.IsCompatibleWith(simplifiedSchema));
        Assert.Equal("Twilio Simple WhatsApp", simplifiedSchema.DisplayName);

        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));
        Assert.True(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
        Assert.False(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(simplifiedSchema.Capabilities.HasFlag(ChannelCapability.Templates));

        Assert.DoesNotContain(simplifiedSchema.Parameters, p => p.Name == "WebhookUrl");
        Assert.DoesNotContain(simplifiedSchema.Parameters, p => p.Name == "StatusCallback");

        Assert.Equal(2, simplifiedSchema.ContentTypes.Count);
        Assert.Contains(MessageContentType.PlainText, simplifiedSchema.ContentTypes);
        Assert.Contains(MessageContentType.Media, simplifiedSchema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Template, simplifiedSchema.ContentTypes);
    }

    [Fact]
    public void Should_BeCorrectlyDerivedFromTwilioWhatsApp_When_WhatsAppTemplatesIsInvoked()
    {
        // Arrange
        // Act
        var baseSchema = TwilioChannelSchemas.TwilioWhatsApp;
        var templateSchema = TwilioChannelSchemas.WhatsAppTemplates;

        // Assert
        Assert.Equal(baseSchema.ChannelProvider, templateSchema.ChannelProvider);
        Assert.Equal(baseSchema.ChannelType, templateSchema.ChannelType);
        Assert.Equal(baseSchema.Version, templateSchema.Version);
        Assert.True(baseSchema.IsCompatibleWith(templateSchema));
        Assert.Equal("Twilio WhatsApp Templates", templateSchema.DisplayName);

        Assert.True(templateSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(templateSchema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.True(templateSchema.Capabilities.HasFlag(ChannelCapability.Templates));
        Assert.True(templateSchema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
        Assert.False(templateSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        Assert.False(templateSchema.Capabilities.HasFlag(ChannelCapability.MediaAttachments));

        Assert.Equal(2, templateSchema.ContentTypes.Count);
        Assert.Contains(MessageContentType.PlainText, templateSchema.ContentTypes);
        Assert.Contains(MessageContentType.Template, templateSchema.ContentTypes);
        Assert.DoesNotContain(MessageContentType.Media, templateSchema.ContentTypes);
    }

    [Fact]
    public void Should_BeCorrectlyDerivedFromTwilioSms_When_NotificationSmsIsInvoked()
    {
        // Arrange
        // Act
        var baseSchema = TwilioChannelSchemas.TwilioSms;
        var notificationSchema = TwilioChannelSchemas.NotificationSms;

        // Assert
        Assert.Equal(baseSchema.ChannelProvider, notificationSchema.ChannelProvider);
        Assert.Equal(baseSchema.ChannelType, notificationSchema.ChannelType);
        Assert.Equal(baseSchema.Version, notificationSchema.Version);
        Assert.True(baseSchema.IsCompatibleWith(notificationSchema));
        Assert.Equal("Twilio Notification SMS", notificationSchema.DisplayName);

        Assert.True(notificationSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(notificationSchema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        Assert.False(notificationSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));

        Assert.DoesNotContain(notificationSchema.Parameters, p => p.Name == "WebhookUrl");
        Assert.DoesNotContain(notificationSchema.ContentTypes, ct => ct == MessageContentType.Media);
    }

    [Fact]
    public void Should_BeCorrectlyDerivedFromTwilioSms_When_BulkSmsIsInvoked()
    {
        // Arrange
        // Act
        var baseSchema = TwilioChannelSchemas.TwilioSms;
        var bulkSchema = TwilioChannelSchemas.BulkSms;

        // Assert
        Assert.Equal(baseSchema.ChannelProvider, bulkSchema.ChannelProvider);
        Assert.Equal(baseSchema.ChannelType, bulkSchema.ChannelType);
        Assert.Equal(baseSchema.Version, bulkSchema.Version);
        Assert.True(baseSchema.IsCompatibleWith(bulkSchema));
        Assert.Equal("Twilio Bulk SMS", bulkSchema.DisplayName);

        Assert.True(bulkSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
        Assert.True(bulkSchema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
        Assert.False(bulkSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));

        var messagingServiceParam = bulkSchema.Parameters.FirstOrDefault(p => p.Name == "MessagingServiceSid");
        Assert.NotNull(messagingServiceParam);
        Assert.True(messagingServiceParam.IsRequired);

        var phoneEndpoint = bulkSchema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.PhoneNumber);
        Assert.NotNull(phoneEndpoint);
        Assert.False(phoneEndpoint.IsRequired);
    }

    #endregion

    #region Validation

    [Fact]
    public void Should_PassValidationAsRestrictionsOfBase_When_AllSchemasIsInvoked()
    {
        // Arrange
        var smsBaseSchema = TwilioChannelSchemas.TwilioSms;
        var whatsAppBaseSchema = TwilioChannelSchemas.TwilioWhatsApp;

        var smsDerivedSchemas = new[]
        {
            TwilioChannelSchemas.SimpleSms,
            TwilioChannelSchemas.NotificationSms,
            TwilioChannelSchemas.BulkSms
        };

        var whatsAppDerivedSchemas = new[]
        {
            TwilioChannelSchemas.SimpleWhatsApp,
            TwilioChannelSchemas.WhatsAppTemplates
        };

        // Act & Assert
        foreach (var derivedSchema in smsDerivedSchemas)
        {
            var validationResults = derivedSchema.ValidateAsRestrictionOf(smsBaseSchema);
            Assert.Empty(validationResults);
        }

        foreach (var derivedSchema in whatsAppDerivedSchemas)
        {
            var validationResults = derivedSchema.ValidateAsRestrictionOf(whatsAppBaseSchema);
            Assert.Empty(validationResults);
        }
    }

    [Fact]
    public void Should_ValidateConnectionSettings_When_AllSchemasIsInvoked()
    {
        // Arrange
        var validSmsSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");

        var validWhatsAppSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");

        var validBulkSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678")
            .SetParameter("MessagingServiceSid", "MG1234567890123456789012345678901234");

        var smsSchemas = new[]
        {
            TwilioChannelSchemas.TwilioSms,
            TwilioChannelSchemas.SimpleSms,
            TwilioChannelSchemas.NotificationSms
        };

        var whatsAppSchemas = new[]
        {
            TwilioChannelSchemas.TwilioWhatsApp,
            TwilioChannelSchemas.SimpleWhatsApp
        };

        // Act & Assert
        foreach (var schema in smsSchemas)
        {
            var validationResults = schema.ValidateConnectionSettings(validSmsSettings);
            Assert.Empty(validationResults);
        }

        foreach (var schema in whatsAppSchemas)
        {
            var validationResults = schema.ValidateConnectionSettings(validWhatsAppSettings);
            Assert.Empty(validationResults);
        }

        var bulkValidationResults = TwilioChannelSchemas.BulkSms.ValidateConnectionSettings(validBulkSettings);
        Assert.Empty(bulkValidationResults);

        var templateValidationResults = TwilioChannelSchemas.WhatsAppTemplates.ValidateConnectionSettings(validWhatsAppSettings);
        Assert.Empty(templateValidationResults);
    }

    [Fact]
    public void Should_ValidateMessageProperties_When_TwilioSmsIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;

        var validProps = new Dictionary<string, object?>
        {
            ["ValidityPeriod"] = 3600,
            ["MaxPrice"] = 0.05m,
            ["ProvideCallback"] = true
        };

        var invalidProps = new Dictionary<string, object?>
        {
            ["ValidityPeriod"] = "invalid",
            ["UnknownProperty"] = "value"
        };

        // Act
        var validMessage = CreateTestMessage(validProps);
        var invalidMessage = CreateTestMessage(invalidProps);
        var validResults = schema.ValidateMessage(validMessage);
        var invalidResults = schema.ValidateMessage(invalidMessage).ToList();

        // Assert
        Assert.Empty(validResults);
        Assert.NotEmpty(invalidResults);
        Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Message property 'ValidityPeriod' has an incompatible type"));
        Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Unknown message property 'UnknownProperty'"));
    }

    [Fact]
    public void Should_ValidateMessageProperties_When_TwilioWhatsAppIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        var validProps = new Dictionary<string, object?>
        {
            ["ProvideCallback"] = true
        };

        var invalidProps = new Dictionary<string, object?>
        {
            ["UnknownProperty"] = "value"
        };

        // Act
        var validMessage = CreateTestMessage(validProps);
        var invalidMessage = CreateTestMessage(invalidProps);
        var validResults = schema.ValidateMessage(validMessage);
        var invalidResults = schema.ValidateMessage(invalidMessage).ToList();

        // Assert
        Assert.Empty(validResults);
        Assert.NotEmpty(invalidResults);
        Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Unknown message property 'UnknownProperty'"));
    }

    [Fact]
    public void Should_HaveConsistentProviderAndChannel_When_AllVersionedSchemasIsInvoked()
    {
        // Arrange
        var smsSchemas = new[]
        {
            TwilioChannelSchemas.TwilioSms,
            TwilioChannelSchemasV6.TwilioSms,
            TwilioChannelSchemasV5.TwilioSms,
        };

        var whatsAppSchemas = new[]
        {
            TwilioChannelSchemas.TwilioWhatsApp,
            TwilioChannelSchemasV6.TwilioWhatsApp,
            TwilioChannelSchemasV5.TwilioWhatsApp,
        };

        // Act & Assert
        Assert.All(smsSchemas, schema =>
        {
            Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
            Assert.Equal(TwilioConnectorConstants.SmsChannel, schema.ChannelType);
        });

        Assert.All(whatsAppSchemas, schema =>
        {
            Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
            Assert.Equal(TwilioConnectorConstants.WhatsAppChannel, schema.ChannelType);
        });
    }

    [Fact]
    public void Should_ReportCorrectVersion_When_AllVersionedSchemasIsInvoked()
    {
        // Arrange
        var expectedVersions = new Dictionary<string, ChannelSchema[]>
        {
            [TwilioConnectorConstants.ConnectorSchemaVersion] =
            [
                TwilioChannelSchemas.TwilioSms,
                TwilioChannelSchemas.TwilioWhatsApp,
                TwilioChannelSchemas.SimpleSms,
                TwilioChannelSchemas.NotificationSms,
                TwilioChannelSchemas.BulkSms,
                TwilioChannelSchemas.SimpleWhatsApp,
                TwilioChannelSchemas.WhatsAppTemplates,
            ],
            [TwilioConnectorConstants.SdkVersion6] =
            [
                TwilioChannelSchemasV6.TwilioSms,
                TwilioChannelSchemasV6.TwilioWhatsApp,
                TwilioChannelSchemasV6.SimpleSms,
                TwilioChannelSchemasV6.NotificationSms,
                TwilioChannelSchemasV6.BulkSms,
                TwilioChannelSchemasV6.SimpleWhatsApp,
                TwilioChannelSchemasV6.WhatsAppTemplates,
            ],
            [TwilioConnectorConstants.SdkVersion5] =
            [
                TwilioChannelSchemasV5.TwilioSms,
                TwilioChannelSchemasV5.TwilioWhatsApp,
                TwilioChannelSchemasV5.SimpleSms,
                TwilioChannelSchemasV5.NotificationSms,
                TwilioChannelSchemasV5.SimpleWhatsApp,
            ],
        };

        // Act & Assert
        foreach (var (expectedVersion, schemas) in expectedVersions)
        {
            Assert.All(schemas, schema => Assert.Equal(expectedVersion, schema.Version));
        }
    }

    #endregion

    #region Helpers

    private static Message CreateTestMessage(IDictionary<string, object?> properties)
    {
        return new Message
        {
            Id = "test-message-id",
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1987654321"),
            Content = new TextContent("Test SMS content"),
            Properties = properties?.ToDictionary(
                kvp => kvp.Key,
                kvp => new MessageProperty(kvp.Key, kvp.Value),
                StringComparer.OrdinalIgnoreCase)
        };
    }

    #endregion
}
