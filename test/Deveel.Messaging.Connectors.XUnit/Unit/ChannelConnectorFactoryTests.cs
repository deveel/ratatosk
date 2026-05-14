//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "ChannelConnectorFactory")]
    public class ChannelConnectorFactoryTests
    {
        [Fact]
        public void Should_ReturnSameInstance_When_SameSettingsProvided()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");
            var schema = CreateSchema();

            var connector1 = factory.Create(settings, schema);
            var connector2 = factory.Create(settings, schema);

            Assert.Same(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstances_When_DifferentSettingsProvided()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var connector1 = factory.Create(CreateSettings("Key", "value1"), schema);
            var connector2 = factory.Create(CreateSettings("Key", "value2"), schema);

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnSameInstance_When_SettingsValuesAreCaseInsensitive()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var connector1 = factory.Create(CreateSettings("APIKEY", "test123"), schema);
            var connector2 = factory.Create(CreateSettings("ApiKey", "test123"), schema);

            Assert.Same(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnSameInstance_When_ParameterOrderDiffers()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var settings1 = CreateSettings(("A", "1"), ("B", "2"));
            var settings2 = CreateSettings(("B", "2"), ("A", "1"));

            var connector1 = factory.Create(settings1, schema);
            var connector2 = factory.Create(settings2, schema);

            Assert.Same(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstance_When_SchemaDiffers()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");

            var connector1 = factory.Create(settings, CreateSchema("ProviderA"));
            var connector2 = factory.Create(settings, CreateSchema("ProviderB"));

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstances_When_ExtraParameterProvided()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var settings1 = CreateSettings("Key", "value");
            var settings2 = CreateSettings(("Key", "value"), ("Extra", "extra"));

            var connector1 = factory.Create(settings1, schema);
            var connector2 = factory.Create(settings2, schema);

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstances_When_ParameterValueDiffers()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var settings1 = CreateSettings(("A", "1"), ("B", "2"));
            var settings2 = CreateSettings(("A", "1"), ("B", "3"));

            var connector1 = factory.Create(settings1, schema);
            var connector2 = factory.Create(settings2, schema);

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstances_When_ParameterRemoved()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var schema = CreateSchema();

            var settings1 = CreateSettings(("A", "1"), ("B", "2"));
            var settings2 = CreateSettings("A", "1");

            var connector1 = factory.Create(settings1, schema);
            var connector2 = factory.Create(settings2, schema);

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnSameInstance_When_AutoDiscoveredSchemaUsedRepeatedly()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");

            var connector1 = factory.Create(settings);
            var connector2 = factory.Create(settings);

            Assert.Same(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnDifferentInstance_When_AutoSchemaAndExplicitSchemaDiffer()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");

            var connector1 = factory.Create(settings, CreateSchema("Explicit"));
            var connector2 = factory.Create(settings);

            Assert.NotSame(connector1, connector2);
        }

        [Fact]
        public void Should_ReturnSameInstance_When_ConcurrentCallsWithSameSettings()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");
            var schema = CreateSchema();

            var results = new IChannelConnector[20];
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.For(0, 20, parallelOptions, i =>
            {
                results[i] = factory.Create(settings, schema);
            });

            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(results[0], results[i]);
            }
        }

        [Fact]
        public void Should_ReturnSameInstance_When_ConcurrentAutoDiscoveredCalls()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            var factory = new ChannelConnectorFactory<PoolTestConnector>(provider);
            var settings = CreateSettings("Key", "value");

            var results = new IChannelConnector[20];
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.For(0, 20, parallelOptions, i =>
            {
                results[i] = factory.Create(settings);
            });

            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(results[0], results[i]);
            }
        }

        private static ConnectionSettings CreateSettings(string key, object? value)
            => new ConnectionSettings(new Dictionary<string, object?> { [key] = value });

        private static ConnectionSettings CreateSettings(params (string key, object? value)[] parameters)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var (key, value) in parameters)
                dict[key] = value;
            return new ConnectionSettings(dict);
        }

        private static IChannelSchema CreateSchema(string? provider = null)
	=> new ChannelSchemaBuilder(provider ?? "Test", "TestType", "1.0.0").Build();

        [ChannelSchema(typeof(PoolTestSchemaFactory))]
        private class PoolTestConnector : ChannelConnectorBase
        {
            public PoolTestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
                : base(schema, settings)
            {
            }

            protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
            {
                SetState(ConnectorState.Ready);
                return ValueTask.CompletedTask;
            }

            protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
                => ValueTask.CompletedTask;

            protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
                => throw new NotImplementedException();
        }

        private class PoolTestSchemaFactory : IChannelSchemaFactory
        {
            public IChannelSchema CreateSchema()
	=> new ChannelSchemaBuilder("PoolTest", "TestType", "1.0.0").Build();
        }
    }
}
