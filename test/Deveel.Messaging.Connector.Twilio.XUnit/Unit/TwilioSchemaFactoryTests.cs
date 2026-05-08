//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Unit tests verifying that <see cref="TwilioSmsSchemaFactory"/> and
/// <see cref="TwilioWhatsAppSchemaFactory"/> return the expected default schemas.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioSchemaFactories")]
public class TwilioSchemaFactoryTests
{
    #region TwilioSmsSchemaFactory

    [Fact]
    public void Should_ReturnTwilioSmsSchema_When_SmsCreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new TwilioSmsSchemaFactory();

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.SmsChannel, schema.ChannelType);
    }

    [Fact]
    public void Should_MatchStaticPropertyValues_When_SmsCreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new TwilioSmsSchemaFactory();
        var expected = TwilioChannelSchemas.TwilioSms;

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.Equal(expected.ChannelProvider, schema.ChannelProvider);
        Assert.Equal(expected.ChannelType, schema.ChannelType);
        Assert.Equal(expected.Version, schema.Version);
    }

    #endregion

    #region TwilioWhatsAppSchemaFactory

    [Fact]
    public void Should_ReturnTwilioWhatsAppSchema_When_WhatsAppCreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new TwilioWhatsAppSchemaFactory();

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal(TwilioConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.WhatsAppChannel, schema.ChannelType);
    }

    [Fact]
    public void Should_MatchStaticPropertyValues_When_WhatsAppCreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new TwilioWhatsAppSchemaFactory();
        var expected = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.Equal(expected.ChannelProvider, schema.ChannelProvider);
        Assert.Equal(expected.ChannelType, schema.ChannelType);
        Assert.Equal(expected.Version, schema.Version);
    }

    #endregion
}



