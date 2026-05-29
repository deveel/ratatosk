using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderValidator")]
public class SenderValidatorTests
{
    private static SenderEntity CreateValidEntity() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "test-sender",
        DisplayName = "Test Sender",
        Address = "+1234567890",
        EndpointType = "phone",
        IsActive = true
    };

    private static async Task<List<ValidationResult>> CollectResults(SenderEntity entity)
    {
        var validator = new SenderValidator();
        var results = new List<ValidationResult>();
        await foreach (var r in validator.ValidateAsync(null!, entity, default))
            results.Add(r);
        return results;
    }

    [Fact]
    public async Task Should_Pass_When_EntityIsValid()
    {
        var results = await CollectResults(CreateValidEntity());
        Assert.Empty(results);
    }

    [Fact]
    public async Task Should_Fail_When_NameIsEmpty()
    {
        var entity = CreateValidEntity();
        entity.Name = "";

        var results = await CollectResults(entity);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public async Task Should_Fail_When_DisplayNameIsEmpty()
    {
        var entity = CreateValidEntity();
        entity.DisplayName = "";

        var results = await CollectResults(entity);
        Assert.Contains(results, r => r.MemberNames.Contains("DisplayName"));
    }

    [Fact]
    public async Task Should_Fail_When_EndpointTypeIsEmpty()
    {
        var entity = CreateValidEntity();
        entity.EndpointType = "";

        var results = await CollectResults(entity);
        Assert.Contains(results, r => r.MemberNames.Contains("EndpointType"));
    }

    [Fact]
    public async Task Should_Fail_When_AddressIsEmpty()
    {
        var entity = CreateValidEntity();
        entity.Address = "";

        var results = await CollectResults(entity);
        Assert.Contains(results, r => r.MemberNames.Contains("Address"));
    }

    [Fact]
    public async Task Should_ReturnMultipleErrors_When_MultipleFieldsInvalid()
    {
        var entity = new SenderEntity();

        var results = await CollectResults(entity);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        Assert.Contains(results, r => r.MemberNames.Contains("DisplayName"));
        Assert.Contains(results, r => r.MemberNames.Contains("EndpointType"));
        Assert.Contains(results, r => r.MemberNames.Contains("Address"));
    }
}
