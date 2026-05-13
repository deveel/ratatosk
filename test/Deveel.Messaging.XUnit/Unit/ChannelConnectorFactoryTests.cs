using Deveel.Messaging.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelConnectorFactory")]
    public class ChannelConnectorFactoryTests
    {
        [Fact]
        public void Should_CreateConnector_WithSettings()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings();

            var connector = factory.Create(settings);

            Assert.NotNull(connector);
            Assert.IsType<MockConnector>(connector);
        }

        [Fact]
        public void Should_PoolConnectors_WithSameSettings()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings()
                .SetParameter("key", "value");

            var c1 = factory.Create(settings);
            var c2 = factory.Create(settings);

            Assert.Same(c1, c2);
        }

        [Fact]
        public void Should_CreateDifferentConnectors_WithDifferentSettings()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var s1 = new ConnectionSettings().SetParameter("key", "A");
            var s2 = new ConnectionSettings().SetParameter("key", "B");

            var c1 = factory.Create(s1);
            var c2 = factory.Create(s2);

            Assert.NotSame(c1, c2);
        }

        [Fact]
        public void Should_CreateWithExplicitSchema()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings();
            var schema = new MockSchemaFactory().CreateSchema();

            var connector = factory.Create(settings, schema);

            Assert.NotNull(connector);
            Assert.Same(schema, connector.Schema);
        }

        [Fact]
        public void Should_Throw_When_SettingsIsNull()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);

            Assert.Throws<ArgumentNullException>(() => factory.Create(null!));
        }

        [Fact]
        public void Should_PoolBySettingsAndSchema()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings().SetParameter("x", "1");
            var schema = new MockSchema();

            var c1 = factory.Create(settings, schema);
            var c2 = factory.Create(new ConnectionSettings().SetParameter("x", "1"), schema);

            Assert.Same(c1, c2);
            Assert.Same(schema, c1.Schema);
        }

        [Fact]
        public void Should_PoolKey_Equality()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1");
            var s2 = new ConnectionSettings().SetParameter("a", "1");
            var schema = new MockSchema();

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(s1, schema);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(s2, schema);

            Assert.Equal(key1, key2);
            Assert.True(key1 == key2);
            Assert.False(key1 != key2);
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        }

        [Fact]
        public void Should_PoolKey_Inequality_DifferentSettings()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1");
            var s2 = new ConnectionSettings().SetParameter("a", "2");
            var schema = new MockSchema();

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(s1, schema);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(s2, schema);

            Assert.NotEqual(key1, key2);
            Assert.True(key1 != key2);
        }

        [Fact]
        public void Should_PoolKey_Inequality_DifferentSchema()
        {
            var settings = new ConnectionSettings().SetParameter("a", "1");
            var s1 = new MockSchema();
            var s2 = new MockSchema();

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(settings, s1);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectorPoolKey(settings, s2);

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void Should_ConnectionSettingsKey_Equality()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1").SetParameter("b", "2");
            var s2 = new ConnectionSettings().SetParameter("b", "2").SetParameter("a", "1");

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s1);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s2);

            Assert.Equal(key1, key2);
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
            Assert.False(key1.Equals("not-a-key"));
        }

        [Fact]
        public void Should_ConnectionSettingsKey_Inequality_DifferentCount()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1");
            var s2 = new ConnectionSettings().SetParameter("a", "1").SetParameter("b", "2");

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s1);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s2);

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void Should_ConnectionSettingsKey_Inequality_DifferentValue()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1");
            var s2 = new ConnectionSettings().SetParameter("a", "2");

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s1);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s2);

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void Should_ConnectionSettingsKey_Inequality_MissingKey()
        {
            var s1 = new ConnectionSettings().SetParameter("a", "1").SetParameter("b", "2");
            var s2 = new ConnectionSettings().SetParameter("a", "1");

            var key1 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s1);
            var key2 = new ChannelConnectorFactory<MockConnector>.ConnectionSettingsKey(s2);

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void Should_NotThrowOnNullServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelConnectorFactory<MockConnector>(null!));
        }
    }
}
