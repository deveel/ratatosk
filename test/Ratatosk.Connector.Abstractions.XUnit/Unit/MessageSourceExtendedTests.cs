using System.Text;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageSource")]
public class MessageSourceExtendedTests
{
    [Fact]
    public void Should_CreateTextSource()
    {
        var source = MessageSource.Text("hello world");
        Assert.Equal("text/plain", source.ContentType);
    }

    [Fact]
    public void Should_ReturnText_When_AsText()
    {
        var source = MessageSource.Text("hello");
        var text = source.AsText();
        Assert.Equal("hello", text);
    }

    [Fact]
    public void Should_ParseJson_When_AsJsonGeneric()
    {
        var source = MessageSource.Json(@"{""key"":""value""}");
        var json = source.AsJson<Dictionary<string, string>>();
        Assert.NotNull(json);
        Assert.Equal("value", json?["key"]);
    }

    [Fact]
    public void Should_ParseUrlPostData_When_ValidFormUrlEncoded()
    {
        var source = MessageSource.UrlPost("field1=value1&field2=value2");
        var data = source.AsUrlPostData();
        Assert.NotNull(data);
        Assert.Equal("value1", data["field1"]);
        Assert.Equal("value2", data["field2"]);
    }

    [Fact]
    public void Should_Throw_When_AsTextOnNonTextContent()
    {
        var source = MessageSource.Binary(new byte[] { 1, 2, 3 });
        Assert.Throws<MessagingException>(() => source.AsText());
    }

    [Fact]
    public void Should_Throw_When_AsJsonOnNonJsonContent()
    {
        var source = MessageSource.Text("hello");
        Assert.Throws<MessagingException>(() => source.AsJson<object>());
    }

    [Fact]
    public void Should_Throw_When_AsUrlPostDataOnNonFormContent()
    {
        var source = MessageSource.Text("hello");
        Assert.Throws<MessagingException>(() => source.AsUrlPostData());
    }

    [Fact]
    public void Should_CreateFromBytes()
    {
        var bytes = Encoding.UTF8.GetBytes("test");
        var source = new MessageSource("application/octet-stream", bytes.AsMemory());
        Assert.Equal("application/octet-stream", source.ContentType);
    }
}
