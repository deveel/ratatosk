//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Unit tests verifying that <see cref="FirebasePushSchemaFactory"/> returns
    /// the expected default schema.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebasePushSchemaFactory")]
    public class FirebasePushSchemaFactoryTests
    {
        [Fact]
        public void Should_ReturnFirebasePushSchema_When_CreateSchemaIsCalled()
        {
            // Arrange
            IChannelSchemaFactory factory = new FirebasePushSchemaFactory();

            // Act
            var schema = factory.CreateSchema();

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(FirebaseConnectorConstants.Provider, schema.ChannelProvider);
            Assert.Equal(FirebaseConnectorConstants.PushChannel, schema.ChannelType);
        }

        [Fact]
        public void Should_MatchStaticPropertyValues_When_CreateSchemaIsCalled()
        {
            // Arrange
            IChannelSchemaFactory factory = new FirebasePushSchemaFactory();
            var expected = FirebaseChannelSchemas.FirebasePush;

            // Act
            var schema = factory.CreateSchema();

            // Assert
            Assert.Equal(expected.ChannelProvider, schema.ChannelProvider);
            Assert.Equal(expected.ChannelType, schema.ChannelType);
            Assert.Equal(expected.Version, schema.Version);
            Assert.Equal(expected.DisplayName, schema.DisplayName);
        }
    }
}


