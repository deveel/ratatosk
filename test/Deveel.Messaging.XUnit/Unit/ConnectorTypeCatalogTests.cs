using Deveel.Messaging.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorTypeCatalog")]
    public class ConnectorTypeCatalogTests
    {
        [Fact]
        public void Should_RegisterEntry_And_LookupByName()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "mock", typeof(MockConnector) });

            var found = catalog.TryGetEntry("mock", out var entry);

            Assert.True(found);
            Assert.NotNull(entry);
            Assert.Equal("mock", entry!.Name);
            Assert.Equal(typeof(MockConnector), entry.ConnectorType);
        }

        [Fact]
        public void Should_LookupByName_CaseInsensitive()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "MockChannel", typeof(MockConnector) });

            var found = catalog.TryGetEntry("mockchannel", out var entry);

            Assert.True(found);
            Assert.NotNull(entry);
            Assert.Equal("MockChannel", entry!.Name);
        }

        [Fact]
        public void Should_ReturnFalse_When_EntryNotFound()
        {
            var catalog = new ConnectorTypeCatalog();

            var found = catalog.TryGetEntry("nonexistent", out var entry);

            Assert.False(found);
            Assert.Null(entry);
        }

        [Fact]
        public void Should_Overwrite_When_SameNameRegisteredTwice()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "dup", typeof(MockConnector) });
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "dup", typeof(MockConnector) });

            var found = catalog.TryGetEntry("dup", out var entry);
            Assert.True(found);
            Assert.NotNull(entry);
        }

        [Fact]
        public void Should_ReturnSchema_FromServiceProvider()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "mock", typeof(MockConnector) });

            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var found = catalog.TryGetEntry("mock", out var entry);
            Assert.True(found);

            var schema = entry!.GetSchema(provider);

            Assert.NotNull(schema);
            Assert.Equal("MockProvider", schema.ChannelProvider);
            Assert.Equal("MockChannel", schema.ChannelType);
        }

        [Fact]
        public void Should_CacheSchema_AfterFirstAccess()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "mock", typeof(MockConnector) });

            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var found = catalog.TryGetEntry("mock", out var entry);
            Assert.True(found);

            var schema1 = entry!.GetSchema(provider);
            var schema2 = entry.GetSchema(provider);

            Assert.Same(schema1, schema2);
        }

        [Fact]
        public void Should_LookupByType()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "mock", typeof(MockConnector) });

            var found = catalog.TryGetEntry(typeof(MockConnector), out var entry);

            Assert.True(found);
            Assert.NotNull(entry);
            Assert.Equal("mock", entry!.Name);
        }

        [Fact]
        public void Should_ReturnFalse_When_LookupByTypeNotFound()
        {
            var catalog = new ConnectorTypeCatalog();

            var found = catalog.TryGetEntry(typeof(MockConnector), out var entry);

            Assert.False(found);
            Assert.Null(entry);
        }

        [Fact]
        public void Should_BeThreadSafe_When_AccessingSchemaConcurrently()
        {
            var catalog = new ConnectorTypeCatalog();
            typeof(ConnectorTypeCatalog).GetMethod("Register", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(catalog, new object[] { "mock", typeof(MockConnector) });

            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var found = catalog.TryGetEntry("mock", out var entry);
            Assert.True(found);

            var schemas = new List<IChannelSchema>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.For(0, 32, parallelOptions, _ =>
            {
                var schema = entry!.GetSchema(provider);
                lock (schemas)
                    schemas.Add(schema);
            });

            Assert.All(schemas, s => Assert.Same(schemas[0], s));
        }
    }
}
