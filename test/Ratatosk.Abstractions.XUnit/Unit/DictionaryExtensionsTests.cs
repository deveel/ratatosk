using System.Text.Json;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "DictionaryExtensions")]
public class DictionaryExtensionsTests
{
    [Fact]
    public void Should_TryGetValue_When_Exists()
    {
        var dict = new Dictionary<string, object> { ["key"] = "hello" };
        var found = dict.TryGetValue<string>("key", out var value);
        Assert.True(found);
        Assert.Equal("hello", value);
    }

    [Fact]
    public void Should_ReturnFalse_When_KeyDoesNotExist()
    {
        var dict = new Dictionary<string, object>();
        var found = dict.TryGetValue<string>("missing", out var value);
        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void Should_ReturnFalse_When_TypeMismatch()
    {
        var dict = new Dictionary<string, object> { ["key"] = new List<int>() };
        var found = dict.TryGetValue<string>("key", out var value);
        Assert.False(found);
        Assert.Null(value);
    }

    enum TestEnum { OptionA, OptionB }

    [Fact]
    public void Should_TryGetEnum_FromString()
    {
        var dict = new Dictionary<string, object> { ["mode"] = "OptionA" };
        var found = dict.TryGetValue<TestEnum>("mode", out var value);
        Assert.True(found);
        Assert.Equal(TestEnum.OptionA, value);
    }

    [Fact]
    public void Should_ReturnFalse_When_EnumParseFails()
    {
        var dict = new Dictionary<string, object> { ["mode"] = "NotARealEnum" };
        var found = dict.TryGetValue<TestEnum>("mode", out var _);
        Assert.False(found);
    }

    [Fact]
    public void Should_ConvertIntToLong()
    {
        var dict = new Dictionary<string, object> { ["val"] = 42 };
        var found = dict.TryGetValue<long>("val", out var value);
        Assert.True(found);
        Assert.Equal(42L, value);
    }

    [Fact]
    public void Should_ConvertDoubleToDecimal()
    {
        var dict = new Dictionary<string, object> { ["val"] = 3.14 };
        var found = dict.TryGetValue<decimal>("val", out var value);
        Assert.True(found);
        Assert.Equal(3.14m, value);
    }

    [Fact]
    public void Should_ReturnFalse_When_FormatException()
    {
        var dict = new Dictionary<string, object> { ["val"] = "not-a-number" };
        var found = dict.TryGetValue<int>("val", out var _);
        Assert.False(found);
    }

    [Fact]
    public void Should_DeserializeJsonElement()
    {
        var json = JsonSerializer.SerializeToElement(new { name = "test" });
        var dict = new Dictionary<string, object> { ["data"] = json };
        var found = dict.TryGetValue<JsonElement>("data", out var value);
        Assert.True(found);
        Assert.Equal("test", value.GetProperty("name").GetString());
    }

    [Fact]
    public void Should_ReturnFalse_When_Overflow()
    {
        var dict = new Dictionary<string, object> { ["val"] = double.MaxValue };
        var found = dict.TryGetValue<int>("val", out var _);
        Assert.False(found);
    }

    [Fact]
    public void Should_MergeTwoDictionaries()
    {
        var a = new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 };
        var b = new Dictionary<string, int> { ["z"] = 3 };
        var merged = a.Merge(b);
        Assert.Equal(3, merged.Count);
        Assert.Equal(1, merged["x"]);
        Assert.Equal(2, merged["y"]);
        Assert.Equal(3, merged["z"]);
    }

    [Fact]
    public void Should_Merge_WhenBothNull()
    {
        var merged = ((IDictionary<string, int>?)null).Merge(null);
        Assert.Empty(merged);
    }

    [Fact]
    public void Should_Merge_WhenFirstNull()
    {
        var b = new Dictionary<string, int> { ["a"] = 1 };
        var merged = ((IDictionary<string, int>?)null).Merge(b);
        Assert.Single(merged);
        Assert.Equal(1, merged["a"]);
    }

    [Fact]
    public void Should_Merge_WhenSecondNull()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        var merged = a.Merge(null);
        Assert.Single(merged);
        Assert.Equal(1, merged["a"]);
    }

    [Fact]
    public void Should_Merge_WithOverwrite()
    {
        var a = new Dictionary<string, int> { ["x"] = 1 };
        var b = new Dictionary<string, int> { ["x"] = 2 };
        var merged = a.Merge(b);
        Assert.Single(merged);
        Assert.Equal(2, merged["x"]);
    }

    [Fact]
    public void Should_Merge_AndRemove_WhenValueIsNull()
    {
        var a = new Dictionary<string, string?> { ["keep"] = "1", ["remove"] = "2" };
        var b = new Dictionary<string, string?> { ["remove"] = null };
        var merged = a.Merge(b);
        Assert.Single(merged);
        Assert.True(merged.ContainsKey("keep"));
    }
}
