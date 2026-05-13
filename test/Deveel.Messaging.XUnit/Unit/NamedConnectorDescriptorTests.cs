using Deveel.Messaging.XUnit.Fixtures;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "NamedConnectorDescriptor")]
    public class NamedConnectorDescriptorTests
    {
        private static IChannelSchema CreateSchema()
            => new MockSchemaFactory().CreateSchema();

        [Fact]
        public void Should_CreateWithNameTypeAndSchema()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("my-channel", typeof(MockConnector), schema);

            Assert.Equal("my-channel", descriptor.Name);
            Assert.Equal(typeof(MockConnector), descriptor.ConnectorType);
            Assert.Same(schema, descriptor.Schema);
        }

        [Fact]
        public void Should_Throw_When_NameIsNull()
        {
            var schema = CreateSchema();
            Assert.Throws<ArgumentNullException>(() => new NamedConnectorDescriptor(null!, typeof(MockConnector), schema));
        }

        [Fact]
        public void Should_Throw_When_NameIsEmpty()
        {
            var schema = CreateSchema();
            Assert.Throws<ArgumentException>(() => new NamedConnectorDescriptor("", typeof(MockConnector), schema));
        }

        [Fact]
        public void Should_Throw_When_ConnectorTypeIsNull()
        {
            var schema = CreateSchema();
            Assert.Throws<ArgumentNullException>(() => new NamedConnectorDescriptor("ch", null!, schema));
        }

        [Fact]
        public void Should_Throw_When_SchemaIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NamedConnectorDescriptor("ch", typeof(MockConnector), null!));
        }

        [Fact]
        public void Should_Throw_When_TypeDoesNotImplementIChannelConnector()
        {
            var schema = CreateSchema();
            Assert.Throws<ArgumentException>(() => new NamedConnectorDescriptor("ch", typeof(string), schema));
        }

        [Fact]
        public void Should_UseEmptySettings_When_NotProvided()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);

            Assert.NotNull(descriptor.Settings);
            Assert.Empty(descriptor.Settings);
        }

        [Fact]
        public void Should_UseProvidedSettings()
        {
            var schema = CreateSchema();
            var settings = new Dictionary<string, object?> { ["key1"] = "val1", ["key2"] = 42 };
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema, settings);

            Assert.Equal(2, descriptor.Settings.Count);
            Assert.Equal("val1", descriptor.Settings["key1"]);
            Assert.Equal(42, descriptor.Settings["key2"]);
        }

        [Fact]
        public void Should_ExposeSchemaProperties()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);

            Assert.Equal(schema.ChannelProvider, descriptor.ChannelProvider);
            Assert.Equal(schema.ChannelType, descriptor.ChannelType);
            Assert.Equal(schema.Version, descriptor.Version);
            Assert.Equal(schema.DisplayName, descriptor.DisplayName);
            Assert.Equal(schema.Capabilities, descriptor.Capabilities);
        }

        [Fact]
        public void Should_GetSetting_When_Exists()
        {
            var schema = CreateSchema();
            var settings = new Dictionary<string, object?> { ["ApiKey"] = "abc-123" };
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema, settings);

            var value = descriptor.GetSetting<string>("ApiKey");
            Assert.Equal("abc-123", value);
        }

        [Fact]
        public void Should_ReturnDefault_When_SettingDoesNotExist()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);

            var value = descriptor.GetSetting<string>("NonExistent");
            Assert.Null(value);
        }

        [Fact]
        public void Should_ReturnDefault_When_SettingTypeMismatch()
        {
            var schema = CreateSchema();
            var settings = new Dictionary<string, object?> { ["Port"] = 8080 };
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema, settings);

            var value = descriptor.GetSetting<string>("Port");
            Assert.Null(value);
        }

        [Fact]
        public void Should_TryGetSetting_When_Exists()
        {
            var schema = CreateSchema();
            var settings = new Dictionary<string, object?> { ["Enabled"] = true };
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema, settings);

            var found = descriptor.TryGetSetting<bool>("Enabled", out var value);

            Assert.True(found);
            Assert.True(value);
        }

        [Fact]
        public void Should_TryGetSetting_When_DoesNotExist()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);

            var found = descriptor.TryGetSetting<string>("Missing", out var value);

            Assert.False(found);
            Assert.Null(value);
        }

        [Fact]
        public void Should_TryGetSetting_When_TypeMismatch()
        {
            var schema = CreateSchema();
            var settings = new Dictionary<string, object?> { ["Count"] = "not-a-number" };
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema, settings);

            var found = descriptor.TryGetSetting<int>("Count", out var value);

            Assert.False(found);
            Assert.Equal(0, value);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var schema = CreateSchema();
            var descriptor = new NamedConnectorDescriptor("my-ch", typeof(MockConnector), schema);

            var str = descriptor.ToString();
            Assert.Contains("my-ch", str);
        }

        [Fact]
        public void Equals_ReturnsTrue_ForSameName()
        {
            var schema = CreateSchema();
            var d1 = new NamedConnectorDescriptor("same", typeof(MockConnector), schema);
            var d2 = new NamedConnectorDescriptor("same", typeof(MockConnector), schema);

            Assert.Equal(d1, d2);
            Assert.True(d1.Equals(d2));
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentName()
        {
            var schema = CreateSchema();
            var d1 = new NamedConnectorDescriptor("a", typeof(MockConnector), schema);
            var d2 = new NamedConnectorDescriptor("b", typeof(MockConnector), schema);

            Assert.NotEqual(d1, d2);
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentType()
        {
            var schema = CreateSchema();
            var d1 = new NamedConnectorDescriptor("x", typeof(MockConnector), schema);

            Assert.False(d1.Equals("not-a-descriptor"));
        }

        [Fact]
        public void GetHashCode_ReturnsSame_ForSameName()
        {
            var schema = CreateSchema();
            var d1 = new NamedConnectorDescriptor("same", typeof(MockConnector), schema);
            var d2 = new NamedConnectorDescriptor("same", typeof(MockConnector), schema);

            Assert.Equal(d1.GetHashCode(), d2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_Differs_ForDifferentName()
        {
            var schema = CreateSchema();
            var d1 = new NamedConnectorDescriptor("a", typeof(MockConnector), schema);
            var d2 = new NamedConnectorDescriptor("b", typeof(MockConnector), schema);

            Assert.NotEqual(d1.GetHashCode(), d2.GetHashCode());
        }
    }
}
