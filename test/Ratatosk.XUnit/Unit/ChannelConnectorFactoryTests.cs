using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ratatosk.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelConnectorFactory")]
    public class ChannelConnectorFactoryTests
    {
        private static IServiceProvider CreateServices()
            => new ServiceCollection()
                .AddSingleton<ILogger<MockConnector>>(NullLogger<MockConnector>.Instance)
                .BuildServiceProvider();

        [Fact]
        public void Should_CreateConnector_WithSettings()
        {
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings();

            var connector = factory.Create(settings);

            Assert.NotNull(connector);
            Assert.IsType<MockConnector>(connector);
        }

        [Fact]
        public void Should_PoolConnectors_WithSameSettings()
        {
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings().SetParameter("key", "value");

            var c1 = factory.Create(settings);
            var c2 = factory.Create(settings);

            Assert.Same(c1, c2);
        }

        [Fact]
        public void Should_CreateDifferentConnectors_WithDifferentSettings()
        {
            var services = CreateServices();
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
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings();
            var schema = new MockSchema();

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
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var schema = new MockSchema();

            var c1 = factory.Create(new ConnectionSettings().SetParameter("x", "1"), schema);
            var c2 = factory.Create(new ConnectionSettings().SetParameter("x", "1"), schema);

            Assert.Same(c1, c2);
            Assert.Same(schema, c1.Schema);
        }

        [Fact]
        public void Should_NotPool_WhenSchemaIsDifferent()
        {
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var settings = new ConnectionSettings().SetParameter("x", "1");

            var c1 = factory.Create(settings, new MockSchema { ChannelProvider = "A" });
            var c2 = factory.Create(settings, new MockSchema { ChannelProvider = "B" });

            Assert.NotSame(c1, c2);
        }

        [Fact]
        public void Should_Pool_WhenSettingsKeyEquality()
        {
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var schema = new MockSchema();

            var c1 = factory.Create(
                new ConnectionSettings().SetParameter("a", "1").SetParameter("b", "2"), schema);
            var c2 = factory.Create(
                new ConnectionSettings().SetParameter("b", "2").SetParameter("a", "1"), schema);

            Assert.Same(c1, c2);
        }

        [Fact]
        public void Should_NotPool_WhenSettingsMissingKeys()
        {
            var services = CreateServices();
            var factory = new ChannelConnectorFactory<MockConnector>(services);
            var schema = new MockSchema();

            var c1 = factory.Create(
                new ConnectionSettings().SetParameter("a", "1").SetParameter("b", "2"), schema);
            var c2 = factory.Create(
                new ConnectionSettings().SetParameter("a", "1"), schema);

            Assert.NotSame(c1, c2);
        }

        [Fact]
        public void Should_NotThrowOnNullServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new ChannelConnectorFactory<MockConnector>(null!));
        }
    }
}
