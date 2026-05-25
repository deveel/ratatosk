namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelSchema")]
    public class ChannelSchemaTests
    {
        [Fact]
        public void Should_SetProperties_When_Constructed()
        {
            var schema = new ChannelSchema("TestProvider", "TestType", "1.0");

            Assert.Equal("TestProvider", schema.ChannelProvider);
            Assert.Equal("TestType", schema.ChannelType);
            Assert.Equal("1.0", schema.Version);
            Assert.True(schema.IsStrict);
            Assert.Equal(ChannelCapability.SendMessages, schema.Capabilities);
            Assert.Empty(schema.Parameters);
            Assert.Empty(schema.MessageProperties);
            Assert.Empty(schema.ContentTypes);
            Assert.Empty(schema.AuthenticationConfigurations);
            Assert.Empty(schema.Endpoints);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ChannelProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchema(null!, "Type", "1.0"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_ChannelProviderIsEmptyOrWhitespace(string provider)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchema(provider, "Type", "1.0"));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ChannelTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchema("Provider", null!, "1.0"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_ChannelTypeIsEmptyOrWhitespace(string type)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchema("Provider", type, "1.0"));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_VersionIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchema("Provider", "Type", null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_VersionIsEmptyOrWhitespace(string version)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchema("Provider", "Type", version));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_CopySourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchema(null!, "Derived"));
        }

        [Fact]
        public void Should_CopyProperties_When_UsingCopyConstructor()
        {
            var source = new ChannelSchema("SourceProvider", "SourceType", "2.0");
            source.DisplayName = "Original Schema";
            source.IsStrict = false;
            source.Capabilities = ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages;

            var copy = new ChannelSchema(source);

            Assert.Equal(source.ChannelProvider, copy.ChannelProvider);
            Assert.Equal(source.ChannelType, copy.ChannelType);
            Assert.Equal(source.Version, copy.Version);
            Assert.Equal(source.IsStrict, copy.IsStrict);
            Assert.Equal(source.Capabilities, copy.Capabilities);
        }

        [Fact]
        public void Should_SetDefaultDisplayName_When_CopyingWithoutDerivedName()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            source.DisplayName = "Original";

            var copy = new ChannelSchema(source);

            Assert.Equal("Original (Copy)", copy.DisplayName);
        }

        [Fact]
        public void Should_UseDerivedDisplayName_When_Provided()
        {
            var source = new ChannelSchema("P", "T", "1.0");

            var copy = new ChannelSchema(source, "Custom Copy");

            Assert.Equal("Custom Copy", copy.DisplayName);
        }

        [Fact]
        public void Should_DeepCopyParameters_When_UsingCopyConstructor()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            var param = new ChannelParameter("ApiKey", DataType.String);
            var field = typeof(ChannelSchema).GetField("parameters",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = field!.GetValue(source) as System.Collections.IList;
            list!.Add(param);

            var copy = new ChannelSchema(source);

            Assert.Single(copy.Parameters);
            Assert.Equal("ApiKey", copy.Parameters[0].Name);
        }

        [Fact]
        public void Should_DeepCopyAuthenticationConfigurations_When_UsingCopyConstructor()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            var authConfig = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key");
            var field = typeof(ChannelSchema).GetField("authenticationConfigurations",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = field!.GetValue(source) as System.Collections.IList;
            list!.Add(authConfig);

            var copy = new ChannelSchema(source);

            Assert.Single(copy.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.ApiKey, copy.AuthenticationConfigurations[0].Scheme);
        }

        [Fact]
        public void Should_DeepCopyContentTypes_When_UsingCopyConstructor()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            var contentType = MessageContentType.PlainText;
            var field = typeof(ChannelSchema).GetField("contentTypes",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = field!.GetValue(source) as System.Collections.IList;
            list!.Add(contentType);

            var copy = new ChannelSchema(source);

            Assert.Single(copy.ContentTypes);
            Assert.Equal(MessageContentType.PlainText, copy.ContentTypes[0]);
        }

        [Fact]
        public void Should_DeepCopyEndpoints_When_UsingCopyConstructor()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            var endpoint = new ChannelEndpointConfiguration(EndpointType.EmailAddress);
            var field = typeof(ChannelSchema).GetField("endpoints",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = field!.GetValue(source) as System.Collections.IList;
            list!.Add(endpoint);

            var copy = new ChannelSchema(source);

            Assert.Single(copy.Endpoints);
            Assert.Equal(EndpointType.EmailAddress, copy.Endpoints[0].Type);
        }

        [Fact]
        public void Should_ReturnDistinctAuthenticationSchemes()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            var authConfig1 = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key");
            var authConfig2 = new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bearer");
            var field = typeof(ChannelSchema).GetField("authenticationConfigurations",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = field!.GetValue(source) as System.Collections.IList;
            list!.Add(authConfig1);
            list!.Add(authConfig2);

            var schemes = source.AuthenticationSchemes.ToList();

            Assert.Equal(2, schemes.Count);
            Assert.Contains(AuthenticationScheme.ApiKey, schemes);
            Assert.Contains(AuthenticationScheme.Bearer, schemes);
        }

        [Fact]
        public void Should_SetDisplayName_When_PropertyIsSet()
        {
            var schema = new ChannelSchema("P", "T", "1.0");
            schema.DisplayName = "My Channel";
            Assert.Equal("My Channel", schema.DisplayName);
        }

        [Fact]
        public void Should_SetIsStrict_When_PropertyIsSet()
        {
            var schema = new ChannelSchema("P", "T", "1.0");
            schema.IsStrict = false;
            Assert.False(schema.IsStrict);
        }

        [Fact]
        public void Should_SetCapabilities_When_PropertyIsSet()
        {
            var schema = new ChannelSchema("P", "T", "1.0");
            schema.Capabilities = ChannelCapability.ReceiveMessages;
            Assert.Equal(ChannelCapability.ReceiveMessages, schema.Capabilities);
        }
    }
}
