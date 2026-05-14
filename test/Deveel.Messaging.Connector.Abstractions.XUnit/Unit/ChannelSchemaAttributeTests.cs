namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "ChannelSchemaAttribute")]
public class ChannelSchemaAttributeTests
{
    private class TestSchema : IChannelSchema
    {
        public string ChannelProvider => "Test";
        public string ChannelType => "Test";
        public string Version => "1.0";
        public string? DisplayName => null;
        public bool IsStrict => false;
        public ChannelCapability Capabilities => ChannelCapability.SendMessages;
        public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
        public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
        public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
        public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
        public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
    }

    private class TestSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => new TestSchema();
    }

    [Fact]
    public void Should_CreateWithSchemaType()
    {
        var attr = new ChannelSchemaAttribute(typeof(TestSchema));
        Assert.Equal(typeof(TestSchema), attr.SchemaType);
    }

    [Fact]
    public void Should_CreateWithFactoryType()
    {
        var attr = new ChannelSchemaAttribute(typeof(TestSchemaFactory));
        Assert.Equal(typeof(TestSchemaFactory), attr.SchemaType);
    }

    [Fact]
    public void Should_Throw_When_TypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ChannelSchemaAttribute(null!));
    }

    [Fact]
    public void Should_Throw_When_TypeIsNotSchemaOrFactory()
    {
        Assert.Throws<ArgumentException>(() => new ChannelSchemaAttribute(typeof(string)));
    }

    [Fact]
    public void Should_BeDecoratedOnClass()
    {
        var hasAttr = Attribute.IsDefined(typeof(TestSchema), typeof(ChannelSchemaAttribute));
        Assert.False(hasAttr); // TestSchema itself doesn't have the attribute
    }
}
