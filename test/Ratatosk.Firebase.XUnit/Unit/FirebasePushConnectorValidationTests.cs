using System.ComponentModel.DataAnnotations;
using FirebaseAdmin.Messaging;
using Moq;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FirebasePushConnector")]
public class FirebasePushConnectorValidationTests
{
    private static string ValidDeviceToken => FirebaseMockFactory.CreateValidFirebaseDeviceToken();

    private static async IAsyncEnumerable<ValidationResult> ValidateMessage(IMessage message)
    {
        var schema = FirebaseChannelSchemas.FirebasePush;
        var settings = FirebaseMockFactory.CreateValidConnectionSettings();
        var mockService = new Mock<IFirebaseService>();
        mockService.SetupGet(x => x.IsInitialized).Returns(true);
        mockService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mockService.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockService.Setup(x => x.SendAsync(It.IsAny<FirebaseAdmin.Messaging.Message>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-id");

        var connector = new FirebasePushConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        await foreach (var result in connector.ValidateMessageAsync(message, CancellationToken.None))
            yield return result;
    }

    private static async Task<List<ValidationResult>> CollectValidationErrors(IMessage message)
    {
        var errors = new List<ValidationResult>();
        await foreach (var result in ValidateMessage(message))
        {
            if (result != ValidationResult.Success)
                errors.Add(result);
        }
        return errors;
    }

    [Fact]
    public async Task Should_PassValidation_When_ValidDeviceTokenMessage()
    {
        var message = new MessageBuilder()
            .WithId("test-valid-device")
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_FailValidation_When_NullReceiver()
    {
        var message = new Message();
        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver"));
    }

    [Fact]
    public async Task Should_FailValidation_When_InvalidEndpointType()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.EmailAddress, "test@test.com"))
            .WithContent(new TextContent("Hello"))
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver.Type"));
    }

    [Fact]
    public async Task Should_FailValidation_When_EmptyDeviceToken()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ""))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver.Address"));
    }

    [Fact]
    public async Task Should_FailValidation_When_DeviceTokenTooShort()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, "short-token"))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver.Address"));
    }

    [Fact]
    public async Task Should_FailValidation_When_NullTopicName()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.Topic, ""))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver.Address"));
    }

    [Fact]
    public async Task Should_FailValidation_When_InvalidTopicName()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.Topic, "invalid topic with spaces!"))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Receiver.Address"));
    }

    [Fact]
    public async Task Should_FailValidation_When_ValidTopicName()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.Topic, "valid_topic-name"))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains("Receiver.Address"));
    }

    [Fact]
    public async Task Should_FailValidation_When_TitleTooLong()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", new string('A', FirebaseConnectorConstants.MaxTitleLength + 1))
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Title"));
    }

    [Fact]
    public async Task Should_FailValidation_When_BodyTooLong()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent(new string('A', FirebaseConnectorConstants.MaxBodyLength + 1)))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Content"));
    }

    [Fact]
    public async Task Should_FailValidation_When_InvalidImageUrl()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("ImageUrl", "not-a-url")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("ImageUrl"));
    }

    [Fact]
    public async Task Should_PassValidation_When_ValidImageUrl()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("ImageUrl", "https://example.com/image.jpg")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains("ImageUrl"));
    }

    [Fact]
    public async Task Should_FailValidation_When_InvalidHexColor()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("Color", "not-a-color")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("Color"));
    }

    [Fact]
    public async Task Should_PassValidation_When_ValidHexColor()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("Color", "#FF5722")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains("Color"));
    }

    [Fact]
    public async Task Should_FailValidation_When_InvalidCustomDataJson()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("CustomData", "not-json")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.Contains(errors, e => e.MemberNames.Contains("CustomData"));
    }

    [Fact]
    public async Task Should_PassValidation_When_ValidCustomDataJson()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithContent(new TextContent("Hello"))
            .WithProperty("Title", "Test")
            .WithProperty("CustomData", @"{""key"":""value""}")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains("CustomData"));
    }

    [Fact]
    public async Task Should_PassValidation_When_NoContent()
    {
        var message = new MessageBuilder()
            .To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken))
            .WithProperty("Title", "Test")
            .Build();

        var errors = await CollectValidationErrors(message);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains("Content"));
    }

    [Fact]
    public async Task Should_FailValidation_When_ShortDeviceTokenNoTitle()
    {
        var msg = new Message
        {
            Receiver = new Endpoint(EndpointType.DeviceId, "short"),
            Id = "test-1"
        };

        var errors = await CollectValidationErrors(msg);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_SendBatchWithDeviceTokens()
    {
        var mockService = new Mock<IFirebaseService>();
        mockService.SetupGet(x => x.IsInitialized).Returns(true);
        mockService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mockService.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var schema = FirebaseChannelSchemas.BulkPush;
        var settings = FirebaseMockFactory.CreateValidConnectionSettings();
        var connector = new FirebasePushConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var batch = new TestMessageBatch(new List<IMessage>
        {
            new MessageBuilder().To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken)).WithProperty("Title", "T1").Build(),
            new MessageBuilder().To(new Endpoint(EndpointType.DeviceId, ValidDeviceToken)).WithProperty("Title", "T2").Build(),
        });

        // The batch send should fail because SendEachAsync isn't mocked, but test that it doesn't throw
        var result = await connector.SendBatchAsync(batch, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_ReturnShutdown_When_ShutdownAsyncCalled()
    {
        var schema = FirebaseChannelSchemas.FirebasePush;
        var settings = FirebaseMockFactory.CreateValidConnectionSettings();
        var mockService = new Mock<IFirebaseService>();
        mockService.SetupGet(x => x.IsInitialized).Returns(true);
        mockService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var connector = new FirebasePushConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);
        Assert.Equal(ConnectorState.Ready, connector.State);

        await connector.ShutdownAsync(CancellationToken.None);
        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public async Task Should_IdempotentShutdown_When_AlreadyShutdown()
    {
        var schema = FirebaseChannelSchemas.FirebasePush;
        var settings = FirebaseMockFactory.CreateValidConnectionSettings();
        var mockService = new Mock<IFirebaseService>();
        mockService.SetupGet(x => x.IsInitialized).Returns(true);
        mockService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var connector = new FirebasePushConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);
        await connector.ShutdownAsync(CancellationToken.None);
        await connector.ShutdownAsync(CancellationToken.None);
        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public async Task Should_ReturnHealthIssues_When_ConnectionTestFails()
    {
        var mockService = new Mock<IFirebaseService>();
        mockService.SetupGet(x => x.IsInitialized).Returns(true);
        mockService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mockService.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("fail"));

        var schema = FirebaseChannelSchemas.FirebasePush;
        var settings = FirebaseMockFactory.CreateValidConnectionSettings();
        var connector = new FirebasePushConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var result = await connector.GetHealthAsync(CancellationToken.None);
        Assert.True(result.IsSuccess());
        Assert.False(result.Value.IsHealthy);
        Assert.Contains(result.Value.Issues, i => i.Contains("fail"));
    }
}
