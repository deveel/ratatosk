using Deveel.Messaging.XUnit.Fixtures;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelSchemaRegistry")]
    public class ChannelSchemaRegistryTests
    {
        private static IChannelSchema CreateSchema(string provider, string type)
        {
            return new MockSchema
            {
                ChannelProvider = provider,
                ChannelType = type
            };
        }

        [Fact]
        public void Should_GetSchemas_FromDirectConnectors()
        {
            var schema = CreateSchema("P1", "T1");
            var connector = new MockConnector(schema);
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                new[] { connector });

            var schemas = registry.GetSchemas().ToList();

            Assert.Contains(schemas, s => s.ChannelProvider == "P1" && s.ChannelType == "T1");
        }

        [Fact]
        public void Should_GetSchemas_FromNamedConnectors()
        {
            var schema = CreateSchema("P2", "T2");
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);
            var registry = new ChannelSchemaRegistry(
                new[] { descriptor },
                Enumerable.Empty<IChannelConnector>());

            var schemas = registry.GetSchemas().ToList();

            Assert.Contains(schemas, s => s.ChannelProvider == "P2" && s.ChannelType == "T2");
        }

        [Fact]
        public void Should_DeduplicateSchemas_ByProviderType()
        {
            var schema = CreateSchema("SameP", "SameT");
            var connector = new MockConnector(schema);
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), schema);
            var registry = new ChannelSchemaRegistry(
                new[] { descriptor },
                new[] { connector });

            var schemas = registry.GetSchemas().ToList();

            Assert.Single(schemas);
        }

        [Fact]
        public void Should_PreferDirectConnectors_OverNamed()
        {
            var directSchema = CreateSchema("P", "T");
            var namedSchema = CreateSchema("P", "T");
            var connector = new MockConnector(directSchema);
            var descriptor = new NamedConnectorDescriptor("ch", typeof(MockConnector), namedSchema);
            var registry = new ChannelSchemaRegistry(
                new[] { descriptor },
                new[] { connector });

            var schemas = registry.GetSchemas().ToList();

            Assert.Single(schemas);
            Assert.Same(directSchema, schemas[0]);
        }

        [Fact]
        public void Should_ReturnMultipleSchemas()
        {
            var s1 = CreateSchema("P1", "T1");
            var s2 = CreateSchema("P2", "T2");
            var c1 = new MockConnector(s1);
            var d2 = new NamedConnectorDescriptor("ch2", typeof(MockConnector), s2);
            var registry = new ChannelSchemaRegistry(
                new[] { d2 },
                new[] { c1 });

            var schemas = registry.GetSchemas().ToList();

            Assert.Equal(2, schemas.Count);
        }

        [Fact]
        public void Should_ReturnEmpty_When_NoConnectors()
        {
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                Enumerable.Empty<IChannelConnector>());

            Assert.Empty(registry.GetSchemas());
        }

        [Fact]
        public void Should_FindSchema_ByProviderAndType()
        {
            var schema = CreateSchema("FindP", "FindT");
            var connector = new MockConnector(schema);
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                new[] { connector });

            var found = registry.FindSchema("FindP", "FindT");

            Assert.NotNull(found);
            Assert.Same(schema, found);
        }

        [Fact]
        public void Should_FindSchema_CaseInsensitive()
        {
            var schema = CreateSchema("PROVIDER", "TYPE");
            var connector = new MockConnector(schema);
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                new[] { connector });

            var found = registry.FindSchema("provider", "type");

            Assert.NotNull(found);
        }

        [Fact]
        public void Should_ReturnNull_When_SchemaNotFound()
        {
            var schema = CreateSchema("P", "T");
            var connector = new MockConnector(schema);
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                new[] { connector });

            var found = registry.FindSchema("NotFound", "Nope");

            Assert.Null(found);
        }

        [Fact]
        public void Should_HaveSchema_When_Exists()
        {
            var schema = CreateSchema("P", "T");
            var connector = new MockConnector(schema);
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                new[] { connector });

            Assert.True(registry.HasSchema("P", "T"));
        }

        [Fact]
        public void Should_NotHaveSchema_When_NotExists()
        {
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                Enumerable.Empty<IChannelConnector>());

            Assert.False(registry.HasSchema("X", "Y"));
        }

        [Fact]
        public void Should_Throw_When_FindSchemaWithNullProvider()
        {
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                Enumerable.Empty<IChannelConnector>());

            Assert.Throws<ArgumentNullException>(() => registry.FindSchema(null!, "T"));
        }

        [Fact]
        public void Should_Throw_When_FindSchemaWithNullType()
        {
            var registry = new ChannelSchemaRegistry(
                Enumerable.Empty<NamedConnectorDescriptor>(),
                Enumerable.Empty<IChannelConnector>());

            Assert.Throws<ArgumentNullException>(() => registry.FindSchema("P", null!));
        }
    }
}
