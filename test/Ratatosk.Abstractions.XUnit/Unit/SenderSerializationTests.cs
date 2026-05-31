using System.Text.Json;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderSerialization")]
public class SenderSerializationTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public void Should_RoundTripPhoneSender_When_SerializedAsIEndpoint()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone", displayName: "My Phone", isActive: true);

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<PhoneSender>(deserialized);
        var phone = (PhoneSender)deserialized!;
        Assert.Equal("+1234567890", phone.PhoneNumber);
        Assert.Equal("my-phone", phone.Name);
        Assert.Equal("My Phone", phone.DisplayName);
        Assert.True(phone.IsActive);
        Assert.Equal(EndpointType.PhoneNumber, phone.Type);
        Assert.Equal("+1234567890", phone.Address);
    }

    [Fact]
    public void Should_RoundTripPhoneSender_When_SerializedAsConcreteType()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone");

        var json = JsonSerializer.Serialize(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PhoneSender>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal("+1234567890", deserialized.PhoneNumber);
        Assert.Equal("my-phone", deserialized.Name);
    }

    [Fact]
    public void Should_RoundTripAlphaNumericSender_When_SerializedAsIEndpoint()
    {
        var sender = new AlphaNumericSender("MyBrand", name: "brand", displayName: "My Brand", isActive: false);

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<AlphaNumericSender>(deserialized);
        var alpha = (AlphaNumericSender)deserialized!;
        Assert.Equal("MyBrand", alpha.BrandName);
        Assert.Equal("brand", alpha.Name);
        Assert.Equal("My Brand", alpha.DisplayName);
        Assert.False(alpha.IsActive);
        Assert.Equal(EndpointType.Label, alpha.Type);
        Assert.Equal("MyBrand", alpha.Address);
    }

    [Fact]
    public void Should_RoundTripEmailSender_When_SerializedAsIEndpoint()
    {
        var sender = new EmailSender("test@example.com", name: "my-email", displayName: "Test User", isActive: false);

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<EmailSender>(deserialized);
        var email = (EmailSender)deserialized!;
        Assert.Equal("test@example.com", email.Address);
        Assert.Equal("Test User", email.DisplayName);
        Assert.Equal("my-email", email.Name);
        Assert.False(email.IsActive);
        Assert.Equal(EndpointType.EmailAddress, email.Type);
    }

    [Fact]
    public void Should_RoundTripBotSender_When_SerializedAsIEndpoint()
    {
        var sender = new BotSender("bot-123", name: "my-bot", displayName: "My Bot", isActive: true);

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<BotSender>(deserialized);
        var bot = (BotSender)deserialized!;
        Assert.Equal("bot-123", bot.PlatformId);
        Assert.Equal("my-bot", bot.Name);
        Assert.Equal("My Bot", bot.DisplayName);
        Assert.True(bot.IsActive);
        Assert.Equal(EndpointType.Id, bot.Type);
        Assert.Equal("bot-123", bot.Address);
    }

    [Fact]
    public void Should_RoundTripSenderRef_When_SerializedAsIEndpoint()
    {
        var sender = new SenderRef("my-sender");

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<SenderRef>(deserialized);
        var senderRef = (SenderRef)deserialized!;
        Assert.Equal("my-sender", senderRef.SenderName);
        Assert.Equal(EndpointType.Any, senderRef.Type);
    }

    [Fact]
    public void Should_IncludeTypeDiscriminator_When_SerializedAsIEndpoint()
    {
        var phone = new PhoneSender("+1234567890");
        var email = new EmailSender("test@example.com");
        var alpha = new AlphaNumericSender("MyBrand");
        var bot = new BotSender("bot-123");
        var senderRef = new SenderRef("my-sender");

        var phoneJson = JsonSerializer.Serialize<IEndpoint>(phone, _jsonOptions);
        var emailJson = JsonSerializer.Serialize<IEndpoint>(email, _jsonOptions);
        var alphaJson = JsonSerializer.Serialize<IEndpoint>(alpha, _jsonOptions);
        var botJson = JsonSerializer.Serialize<IEndpoint>(bot, _jsonOptions);
        var refJson = JsonSerializer.Serialize<IEndpoint>(senderRef, _jsonOptions);

        Assert.Contains("phone", phoneJson);
        Assert.Contains("email", emailJson);
        Assert.Contains("alphanumeric", alphaJson);
        Assert.Contains("bot", botJson);
        Assert.Contains("senderref", refJson);
    }

    [Fact]
    public void Should_PreserveSenderType_When_MessageWithSenderRoundTrips()
    {
        var original = new Message
        {
            Id = "msg-1",
            Sender = new PhoneSender("+1234567890", name: "my-phone"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+0987654321"),
            Content = new TextContent("Hello")
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Sender);
        Assert.IsType<PhoneSender>(deserialized.Sender);
        var phone = (PhoneSender)deserialized.Sender;
        Assert.Equal("+1234567890", phone.PhoneNumber);
        Assert.Equal("my-phone", phone.Name);

        Assert.NotNull(deserialized.Receiver);
        Assert.IsType<Endpoint>(deserialized.Receiver);
        Assert.Equal("+0987654321", deserialized.Receiver.Address);
    }

    [Fact]
    public void Should_PreserveSenderRef_When_MessageWithSenderRefRoundTrips()
    {
        var original = new Message
        {
            Id = "msg-2",
            Sender = new SenderRef("deferred-sender"),
            Content = new TextContent("Hello")
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Sender);
        Assert.IsType<SenderRef>(deserialized.Sender);
        var senderRef = (SenderRef)deserialized.Sender;
        Assert.Equal("deferred-sender", senderRef.SenderName);
    }

    [Fact]
    public void Should_PreserveNullSender_When_MessageWithNullSenderRoundTrips()
    {
        var original = new Message
        {
            Id = "msg-3",
            Sender = null,
            Content = new TextContent("Hello")
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Sender);
    }

    [Fact]
    public void Should_DeserializePhoneSender_When_KnownJson()
    {
        var json = """{"$type":"phone","phoneNumber":"+1234567890","name":"my-phone","displayName":"My Phone","isActive":true}""";

        var result = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<PhoneSender>(result);
        var phone = (PhoneSender)result!;
        Assert.Equal("+1234567890", phone.PhoneNumber);
        Assert.Equal("my-phone", phone.Name);
        Assert.Equal("My Phone", phone.DisplayName);
        Assert.True(phone.IsActive);
    }

    [Fact]
    public void Should_DeserializeSenderRef_When_KnownJson()
    {
        var json = """{"$type":"senderref","senderName":"my-sender"}""";

        var result = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<SenderRef>(result);
        var senderRef = (SenderRef)result!;
        Assert.Equal("my-sender", senderRef.SenderName);
    }

    [Fact]
    public void Should_DeserializeEmailSender_When_KnownJson()
    {
        var json = """{"$type":"email","address":"test@example.com","name":"my-email","displayName":"Test User","isActive":false}""";

        var result = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<EmailSender>(result);
        var email = (EmailSender)result!;
        Assert.Equal("test@example.com", email.Address);
        Assert.Equal("Test User", email.DisplayName);
        Assert.Equal("my-email", email.Name);
        Assert.False(email.IsActive);
    }

    [Fact]
    public void Should_DeserializeBotSender_When_KnownJson()
    {
        var json = """{"$type":"bot","platformId":"bot-123","name":"my-bot","displayName":"My Bot","isActive":true}""";

        var result = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<BotSender>(result);
        var bot = (BotSender)result!;
        Assert.Equal("bot-123", bot.PlatformId);
        Assert.Equal("my-bot", bot.Name);
        Assert.Equal("My Bot", bot.DisplayName);
        Assert.True(bot.IsActive);
    }

    [Fact]
    public void Should_DeserializeAlphaNumericSender_When_KnownJson()
    {
        var json = """{"$type":"alphanumeric","brandName":"MyBrand","name":"brand","displayName":"My Brand","isActive":false}""";

        var result = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<AlphaNumericSender>(result);
        var alpha = (AlphaNumericSender)result!;
        Assert.Equal("MyBrand", alpha.BrandName);
        Assert.Equal("brand", alpha.Name);
        Assert.Equal("My Brand", alpha.DisplayName);
        Assert.False(alpha.IsActive);
    }

    [Fact]
    public void Should_SerializeDefaultValues_When_PhoneSenderWithMinimalConstructor()
    {
        var sender = new PhoneSender("+1234567890");

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<PhoneSender>(deserialized);
        var phone = (PhoneSender)deserialized!;
        Assert.Equal("+1234567890", phone.PhoneNumber);
        Assert.Equal("+1234567890", phone.Name);
        Assert.Equal("+1234567890", phone.DisplayName);
        Assert.True(phone.IsActive);
    }

    [Fact]
    public void Should_SerializeDefaultValues_When_EmailSenderWithMinimalConstructor()
    {
        var sender = new EmailSender("test@example.com");

        var json = JsonSerializer.Serialize<IEndpoint>(sender, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IEndpoint>(json, _jsonOptions);

        Assert.IsType<EmailSender>(deserialized);
        var email = (EmailSender)deserialized!;
        Assert.Equal("test@example.com", email.Address);
        Assert.Equal("test@example.com", email.Name);
        Assert.Equal("test@example.com", email.DisplayName);
        Assert.True(email.IsActive);
    }
}
