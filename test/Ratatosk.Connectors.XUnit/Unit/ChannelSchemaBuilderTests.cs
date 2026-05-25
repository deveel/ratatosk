namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelSchemaBuilder")]
    public class ChannelSchemaBuilderTests
    {
        [Fact]
        public void Should_SetProperties_When_Constructed()
        {
            var builder = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0");
            var schema = builder.Build();

            Assert.NotNull(schema);
            Assert.Equal("TestProvider", schema.ChannelProvider);
            Assert.Equal("TestType", schema.ChannelType);
            Assert.Equal("1.0", schema.Version);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ChannelProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchemaBuilder(null!, "Type", "1.0"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_ChannelProviderIsEmptyOrWhitespace(string provider)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchemaBuilder(provider, "Type", "1.0"));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ChannelTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchemaBuilder("Provider", null!, "1.0"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_ChannelTypeIsEmptyOrWhitespace(string type)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchemaBuilder("Provider", type, "1.0"));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_VersionIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelSchemaBuilder("Provider", "Type", null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_ThrowArgumentException_When_VersionIsEmptyOrWhitespace(string version)
        {
            Assert.Throws<ArgumentException>(() => new ChannelSchemaBuilder("Provider", "Type", version));
        }

        [Fact]
        public void Should_SetDisplayName()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithDisplayName("My Channel")
                .Build();

            Assert.Equal("My Channel", schema.DisplayName);
        }

        [Fact]
        public void Should_SetCapabilities()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
                .Build();

            Assert.Equal(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages, schema.Capabilities);
        }

        [Fact]
        public void Should_AddCapability()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithCapability(ChannelCapability.ReceiveMessages)
                .Build();

            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        }

        [Fact]
        public void Should_RemoveCapability()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithCapability(ChannelCapability.ReceiveMessages)
                .RemoveCapability(ChannelCapability.ReceiveMessages)
                .Build();

            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        }

        [Fact]
        public void Should_RestrictCapabilities()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithCapability(ChannelCapability.ReceiveMessages)
                .WithCapability(ChannelCapability.BulkMessaging)
                .RestrictCapabilities(ChannelCapability.SendMessages)
                .Build();

            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
        }

        [Fact]
        public void Should_AddParameter()
        {
            var param = new ChannelParameter("ApiKey", DataType.String) { IsRequired = true };
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddParameter(param)
                .Build();

            Assert.Single(schema.Parameters);
            Assert.Equal("ApiKey", schema.Parameters[0].Name);
            Assert.True(schema.Parameters[0].IsRequired);
        }

        [Fact]
        public void Should_AddParameterUsingFactory()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddParameter("ApiKey", DataType.String, p => p.IsRequired = true)
                .Build();

            Assert.Single(schema.Parameters);
            Assert.Equal("ApiKey", schema.Parameters[0].Name);
            Assert.True(schema.Parameters[0].IsRequired);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_AddingParameterWithNullName()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<ArgumentNullException>(() => builder.AddParameter(null!, DataType.String));
        }

        [Fact]
        public void Should_AddRequiredParameter()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddRequiredParameter("ApiKey", DataType.String, sensitive: true)
                .Build();

            Assert.Single(schema.Parameters);
            Assert.Equal("ApiKey", schema.Parameters[0].Name);
            Assert.True(schema.Parameters[0].IsRequired);
            Assert.True(schema.Parameters[0].IsSensitive);
        }

        [Fact]
        public void Should_AddRequiredParameterNonSensitive()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddRequiredParameter("Username", DataType.String)
                .Build();

            Assert.Single(schema.Parameters);
            Assert.False(schema.Parameters[0].IsSensitive);
        }

        [Fact]
        public void Should_RemoveParameter()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddParameter("ApiKey", DataType.String)
                .RemoveParameter("ApiKey")
                .Build();

            Assert.Empty(schema.Parameters);
        }

        [Fact]
        public void Should_NotThrow_When_RemovingUnknownParameter()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .RemoveParameter("NonExistent")
                .Build();

            Assert.Empty(schema.Parameters);
        }

        [Fact]
        public void Should_UpdateParameter()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddRequiredParameter("ApiKey", DataType.String)
                .UpdateParameter("ApiKey", p => p.IsRequired = false)
                .Build();

            Assert.False(schema.Parameters[0].IsRequired);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_UpdatingUnknownParameter()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<InvalidOperationException>(() => builder.UpdateParameter("NonExistent", p => { }));
        }

        [Fact]
        public void Should_AddMessageProperty()
        {
            var prop = new MessagePropertyConfiguration("Subject", DataType.String) { IsRequired = true };
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddMessageProperty(prop)
                .Build();

            Assert.Single(schema.MessageProperties);
            Assert.Equal("Subject", schema.MessageProperties[0].Name);
            Assert.True(schema.MessageProperties[0].IsRequired);
        }

        [Fact]
        public void Should_AddMessagePropertyUsingFactory()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddMessageProperty("Subject", DataType.String, p => p.IsRequired = true)
                .Build();

            Assert.Single(schema.MessageProperties);
            Assert.True(schema.MessageProperties[0].IsRequired);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_AddingDuplicateMessageProperty()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            builder.AddMessageProperty("Subject", DataType.String);
            Assert.Throws<InvalidOperationException>(() => builder.AddMessageProperty("Subject", DataType.String));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_AddingMessagePropertyWithNullName()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<ArgumentNullException>(() => builder.AddMessageProperty(null!, DataType.String));
        }

        [Fact]
        public void Should_RemoveMessageProperty()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddMessageProperty("Subject", DataType.String)
                .RemoveMessageProperty("Subject")
                .Build();

            Assert.Empty(schema.MessageProperties);
        }

        [Fact]
        public void Should_NotThrow_When_RemovingUnknownMessageProperty()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .RemoveMessageProperty("NonExistent")
                .Build();

            Assert.Empty(schema.MessageProperties);
        }

        [Fact]
        public void Should_UpdateMessageProperty()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddMessageProperty("Subject", DataType.String)
                .UpdateMessageProperty("Subject", p => p.IsRequired = true)
                .Build();

            Assert.True(schema.MessageProperties[0].IsRequired);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_UpdatingUnknownMessageProperty()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<InvalidOperationException>(() => builder.UpdateMessageProperty("NonExistent", p => { }));
        }

        [Fact]
        public void Should_AddContentType()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddContentType(MessageContentType.PlainText)
                .Build();

            Assert.Single(schema.ContentTypes);
            Assert.Equal(MessageContentType.PlainText, schema.ContentTypes[0]);
        }

        [Fact]
        public void Should_RemoveContentType()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddContentType(MessageContentType.PlainText)
                .RemoveContentType(MessageContentType.PlainText)
                .Build();

            Assert.Empty(schema.ContentTypes);
        }

        [Fact]
        public void Should_RestrictContentTypes()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddContentType(MessageContentType.PlainText)
                .AddContentType(MessageContentType.Html)
                .RestrictContentTypes(MessageContentType.PlainText)
                .Build();

            Assert.Single(schema.ContentTypes);
            Assert.Equal(MessageContentType.PlainText, schema.ContentTypes[0]);
        }

        [Fact]
        public void Should_AddAuthenticationScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.ApiKey)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.ApiKey, schema.AuthenticationConfigurations[0].Scheme);
        }

        [Fact]
        public void Should_AddAuthenticationType()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationType(AuthenticationScheme.Bearer)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.Bearer, schema.AuthenticationConfigurations[0].Scheme);
        }

        [Fact]
        public void Should_AddNoneAuthenticationScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.None)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.None, schema.AuthenticationConfigurations[0].Scheme);
            Assert.Equal("No Authentication", schema.AuthenticationConfigurations[0].DisplayName);
        }

        [Fact]
        public void Should_AddBasicAuthenticationScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.Basic)
                .Build();

            var config = schema.AuthenticationConfigurations[0];
            Assert.Equal(AuthenticationScheme.Basic, config.Scheme);
            Assert.Contains(config.Fields, f => f.FieldName == "Username" && f.AuthenticationRole == "principal");
            Assert.Contains(config.Fields, f => f.FieldName == "Password" && f.AuthenticationRole == "credential");
        }

        [Fact]
        public void Should_AddOAuthClientCredentialsScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.OAuthClientCredentials)
                .Build();

            var config = schema.AuthenticationConfigurations[0];
            Assert.Equal(AuthenticationScheme.OAuthClientCredentials, config.Scheme);
            Assert.Contains(config.Fields, f => f.FieldName == "ClientId");
            Assert.Contains(config.Fields, f => f.FieldName == "ClientSecret");
        }

        [Fact]
        public void Should_AddCertificateScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.Certificate)
                .Build();

            var config = schema.AuthenticationConfigurations[0];
            Assert.Equal(AuthenticationScheme.Certificate, config.Scheme);
            Assert.Contains(config.Fields, f => f.FieldName == "Certificate");
        }

        [Fact]
        public void Should_AddDigestScheme()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.Digest)
                .Build();

            var config = schema.AuthenticationConfigurations[0];
            Assert.Equal(AuthenticationScheme.Digest, config.Scheme);
            Assert.Contains(config.Fields, f => f.FieldName == "Username");
            Assert.Contains(config.Fields, f => f.FieldName == "Password");
            Assert.Contains(config.Fields, f => f.FieldName == "Realm");
        }

        [Fact]
        public void Should_AddAuthenticationConfiguration()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Custom API Key");
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationConfiguration(config)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.ApiKey, schema.AuthenticationConfigurations[0].Scheme);
        }

        [Fact]
        public void Should_AddAuthenticationConfigurationWithFactory()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationConfiguration(() =>
                    new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Custom API Key"))
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_AddingDuplicateAuthenticationScheme()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            builder.AddAuthenticationScheme(AuthenticationScheme.ApiKey);
            Assert.Throws<InvalidOperationException>(() =>
                builder.AddAuthenticationConfiguration(
                    new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Duplicate")));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_AddingNullAuthenticationConfiguration()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<ArgumentNullException>(() => builder.AddAuthenticationConfiguration((AuthenticationConfiguration)null!));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_AddingNullAuthenticationConfigurationFactory()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<ArgumentNullException>(() => builder.AddAuthenticationConfiguration((Func<AuthenticationConfiguration>)null!));
        }

        [Fact]
        public void Should_AddSchemaParameter_When_AuthenticationConfigurationHasPrincipalField()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
                .WithField("ApiKey", DataType.String, f => f.AuthenticationRole = "principal");

            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationConfiguration(config)
                .Build();

            Assert.Single(schema.Parameters);
            Assert.Equal("ApiKey", schema.Parameters[0].Name);
        }

        [Fact]
        public void Should_NotAddDuplicateParameter_When_PrincipalFieldAlreadyExists()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
                .WithField("ApiKey", DataType.String, f => f.AuthenticationRole = "principal");

            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddParameter("ApiKey", DataType.String)
                .AddAuthenticationConfiguration(config)
                .Build();

            Assert.Single(schema.Parameters);
        }

        [Fact]
        public void Should_RestrictAuthenticationSchemes()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.ApiKey)
                .AddAuthenticationScheme(AuthenticationScheme.Bearer)
                .RestrictAuthenticationSchemes(AuthenticationScheme.ApiKey)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationScheme.ApiKey, schema.AuthenticationConfigurations[0].Scheme);
        }

        [Fact]
        public void Should_RestrictAuthenticationConfigurations()
        {
            var apiKeyConfig = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key");
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AddAuthenticationScheme(AuthenticationScheme.ApiKey)
                .AddAuthenticationScheme(AuthenticationScheme.Bearer)
                .RestrictAuthenticationConfigurations(apiKeyConfig)
                .Build();

            Assert.Single(schema.AuthenticationConfigurations);
        }

        [Fact]
        public void Should_HandlesMessageEndpoint()
        {
            var endpoint = new ChannelEndpointConfiguration(EndpointType.EmailAddress) { CanSend = true };
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .HandlesMessageEndpoint(endpoint)
                .Build();

            Assert.Single(schema.Endpoints);
            Assert.Equal(EndpointType.EmailAddress, schema.Endpoints[0].Type);
        }

        [Fact]
        public void Should_HandlesMessageEndpointUsingFactory()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .HandlesMessageEndpoint(EndpointType.EmailAddress, e => e.CanSend = true)
                .Build();

            Assert.Single(schema.Endpoints);
            Assert.True(schema.Endpoints[0].CanSend);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_AddingDuplicateEndpoint()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            builder.HandlesMessageEndpoint(EndpointType.EmailAddress);
            Assert.Throws<InvalidOperationException>(() =>
                builder.HandlesMessageEndpoint(new ChannelEndpointConfiguration(EndpointType.EmailAddress)));
        }

        [Fact]
        public void Should_AllowAnyMessageEndpoint()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .AllowsAnyMessageEndpoint()
                .Build();

            Assert.Single(schema.Endpoints);
            Assert.Equal(EndpointType.Any, schema.Endpoints[0].Type);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_AddingDuplicateAnyEndpoint()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            builder.AllowsAnyMessageEndpoint();
            Assert.Throws<InvalidOperationException>(() => builder.AllowsAnyMessageEndpoint());
        }

        [Fact]
        public void Should_RemoveEndpoint()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .HandlesMessageEndpoint(EndpointType.EmailAddress)
                .RemoveEndpoint(EndpointType.EmailAddress)
                .Build();

            Assert.Empty(schema.Endpoints);
        }

        [Fact]
        public void Should_NotThrow_When_RemovingUnknownEndpoint()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .RemoveEndpoint(EndpointType.EmailAddress)
                .Build();

            Assert.Empty(schema.Endpoints);
        }

        [Fact]
        public void Should_UpdateEndpoint()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .HandlesMessageEndpoint(EndpointType.EmailAddress)
                .UpdateEndpoint(EndpointType.EmailAddress, e => e.CanSend = true)
                .Build();

            Assert.True(schema.Endpoints[0].CanSend);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_UpdatingUnknownEndpoint()
        {
            var builder = new ChannelSchemaBuilder("P", "T", "1.0");
            Assert.Throws<InvalidOperationException>(() => builder.UpdateEndpoint(EndpointType.PhoneNumber, e => { }));
        }

        [Fact]
        public void Should_EnableStrictMode()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithFlexibleMode()
                .WithStrictMode()
                .Build();

            Assert.True(schema.IsStrict);
        }

        [Fact]
        public void Should_EnableFlexibleMode()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithFlexibleMode()
                .Build();

            Assert.False(schema.IsStrict);
        }

        [Fact]
        public void Should_NotThrow_When_WithIdIsCalled()
        {
            var schema = new ChannelSchemaBuilder("P", "T", "1.0")
                .WithId("test-id")
                .Build();

            Assert.NotNull(schema);
        }

        [Fact]
        public void Should_BuildSchemaFromExistingSchema()
        {
            var source = new ChannelSchema("SourceProvider", "SourceType", "2.0");
            source.DisplayName = "Original Schema";

            var schema = ChannelSchemaBuilder.From(source)
                .WithDisplayName("Custom Display")
                .Build();

            Assert.Equal("SourceProvider", schema.ChannelProvider);
            Assert.Equal("SourceType", schema.ChannelType);
            Assert.Equal("2.0", schema.Version);
            Assert.Equal("Custom Display", schema.DisplayName);
        }

        [Fact]
        public void Should_BuildFromSource_WithDefaultCopyDisplayName()
        {
            var source = new ChannelSchema("P", "T", "1.0");
            source.DisplayName = "Original";

            var schema = ChannelSchemaBuilder.From(source).Build();

            Assert.Equal("Original (Copy)", schema.DisplayName);
        }
    }
}
