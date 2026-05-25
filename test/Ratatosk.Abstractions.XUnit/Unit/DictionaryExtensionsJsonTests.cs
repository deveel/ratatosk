using System.Text.Json;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "DictionaryExtensions")]
public class DictionaryExtensionsJsonTests
{
    [Fact]
    public void Should_DeserializeJsonElement_ToObject()
    {
        var json = JsonSerializer.SerializeToElement(new { name = "test", value = 42 });
        var dict = new Dictionary<string, object> { ["data"] = json };
        var found = dict.TryGetValue<JsonElement>("data", out var value);
        Assert.True(found);
        Assert.Equal("test", value.GetProperty("name").GetString());
        Assert.Equal(42, value.GetProperty("value").GetInt32());
    }

    [Fact]
    public void Should_ReturnFalse_When_JsonDeserializationFails()
    {
        var json = JsonSerializer.SerializeToElement("plain string");
        var dict = new Dictionary<string, object> { ["data"] = json };
        var found = dict.TryGetValue<int>("data", out var _);
        Assert.False(found);
    }

    [Fact]
    public void Should_ReturnFalse_When_JsonDeserializationThrowsNotSupported()
    {
        var json = JsonSerializer.SerializeToElement(new { });
        var dict = new Dictionary<string, object> { ["data"] = json };
        var found = dict.TryGetValue<System.IO.Stream>("data", out var _);
        Assert.False(found);
    }

    [Fact]
    public void Should_ConvertIntToDouble()
    {
        var dict = new Dictionary<string, object> { ["val"] = 42 };
        var found = dict.TryGetValue<double>("val", out var value);
        Assert.True(found);
        Assert.Equal(42.0, value);
    }

    [Fact]
    public void Should_ConvertStringToInt()
    {
        var dict = new Dictionary<string, object> { ["val"] = "123" };
        var found = dict.TryGetValue<int>("val", out var value);
        Assert.True(found);
        Assert.Equal(123, value);
    }

    [Fact]
    public void Should_ReturnFalse_When_InvalidCast()
    {
        var dict = new Dictionary<string, object> { ["val"] = Guid.NewGuid() };
        var found = dict.TryGetValue<int>("val", out var _);
        Assert.False(found);
    }
}
