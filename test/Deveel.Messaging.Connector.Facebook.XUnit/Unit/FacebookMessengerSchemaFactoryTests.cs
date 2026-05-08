//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging;

/// <summary>
/// Unit tests verifying that <see cref="FacebookMessengerSchemaFactory"/> returns
/// the expected default schema.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FacebookMessengerSchemaFactory")]
public class FacebookMessengerSchemaFactoryTests
{
    [Fact]
    public void Should_ReturnFacebookMessengerSchema_When_CreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new FacebookMessengerSchemaFactory();

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal(FacebookConnectorConstants.Provider, schema.ChannelProvider);
        Assert.Equal(FacebookConnectorConstants.MessengerChannel, schema.ChannelType);
    }

    [Fact]
    public void Should_MatchStaticPropertyValues_When_CreateSchemaIsCalled()
    {
        // Arrange
        IChannelSchemaFactory factory = new FacebookMessengerSchemaFactory();
        var expected = FacebookChannelSchemas.FacebookMessenger;

        // Act
        var schema = factory.CreateSchema();

        // Assert
        Assert.Equal(expected.ChannelProvider, schema.ChannelProvider);
        Assert.Equal(expected.ChannelType, schema.ChannelType);
        Assert.Equal(expected.Version, schema.Version);
        Assert.Equal(expected.DisplayName, schema.DisplayName);
    }
}


