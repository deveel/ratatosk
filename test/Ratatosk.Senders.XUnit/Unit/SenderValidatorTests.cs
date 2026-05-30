using System.ComponentModel.DataAnnotations;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderValidator")]
public class SenderValidatorTests
{
    private static Sender CreateValidSender() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "test-sender",
        DisplayName = "Test Sender",
        Address = "+1234567890",
        EndpointType = EndpointType.PhoneNumber,
        IsActive = true
    };

    private static async Task<List<ValidationResult>> CollectResults(Sender sender)
    {
        var validator = new SenderValidator<Sender>();
        var results = new List<ValidationResult>();
        await foreach (var r in validator.ValidateAsync(null!, sender, default))
            results.Add(r);
        return results;
    }

    [Fact]
    public async Task Should_Pass_When_SenderIsValid()
    {
        var results = await CollectResults(CreateValidSender());
        Assert.Empty(results);
    }

    [Fact]
    public async Task Should_Fail_When_NameIsEmpty()
    {
        var sender = CreateValidSender();
        sender.Name = "";

        var results = await CollectResults(sender);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public async Task Should_Fail_When_DisplayNameIsEmpty()
    {
        var sender = CreateValidSender();
        sender.DisplayName = "";

        var results = await CollectResults(sender);
        Assert.Contains(results, r => r.MemberNames.Contains("DisplayName"));
    }

    [Fact]
    public async Task Should_Fail_When_EndpointTypeIsAny()
    {
        var sender = CreateValidSender();
        sender.EndpointType = EndpointType.Any;

        var results = await CollectResults(sender);
        Assert.Contains(results, r => r.MemberNames.Contains("EndpointType"));
    }

    [Fact]
    public async Task Should_Fail_When_AddressIsEmpty()
    {
        var sender = CreateValidSender();
        sender.Address = "";

        var results = await CollectResults(sender);
        Assert.Contains(results, r => r.MemberNames.Contains("Address"));
    }

    [Fact]
    public async Task Should_ReturnMultipleErrors_When_MultipleFieldsInvalid()
    {
        var sender = new Sender();

        var results = await CollectResults(sender);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        Assert.Contains(results, r => r.MemberNames.Contains("DisplayName"));
        Assert.Contains(results, r => r.MemberNames.Contains("EndpointType"));
        Assert.Contains(results, r => r.MemberNames.Contains("Address"));
    }
}
