using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid.Helpers.Mail;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SendGridMessageBuilder")]
public class SendGridMessageBuilderTests
{
    private static readonly Type BuilderType = typeof(SendGridService).Assembly
        .GetType("Ratatosk.SendGridMessageBuilder")
        ?? throw new InvalidOperationException("Could not find SendGridMessageBuilder type");

    private static readonly ConstructorInfo BuilderCtor = BuilderType
        .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .First();

    private static readonly MethodInfo SetMessageContentAsyncMethod = BuilderType
        .GetMethod("SetMessageContentAsync", BindingFlags.Public | BindingFlags.Instance)!;

    private static readonly MethodInfo ApplyMessageSettingsMethod = BuilderType
        .GetMethod("ApplyMessageSettings", BindingFlags.Public | BindingFlags.Instance)!;

    private static readonly MethodInfo ApplyConnectorSettingsMethod = BuilderType
        .GetMethod("ApplyConnectorSettings", BindingFlags.Public | BindingFlags.Instance)!;

    private static object CreateBuilder(
        bool sandboxMode = false,
        bool trackingSettings = false,
        string? defaultReplyTo = null,
        ILogger? logger = null)
    {
        return BuilderCtor.Invoke(new object?[] { sandboxMode, trackingSettings, defaultReplyTo, logger });
    }

    private static async Task InvokeSetMessageContentAsync(object builder, SendGridMessage message, IMessage msg)
    {
        await (Task)SetMessageContentAsyncMethod.Invoke(builder, new object[] { message, msg })!;
    }

    private static void InvokeApplyMessageSettings(object builder, SendGridMessage message, IMessage msg, Dictionary<string, object?> properties)
    {
        ApplyMessageSettingsMethod.Invoke(builder, new object[] { message, msg, properties });
    }

    private static void InvokeApplyConnectorSettings(object builder, SendGridMessage message)
    {
        ApplyConnectorSettingsMethod.Invoke(builder, new object[] { message });
    }

    #region SetMessageContentAsync

    [Fact]
    public async Task Should_SkipContent_When_MessageContentIsNull()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var msg = new Message
        {
            Id = "msg-1",
            Content = null,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);
    }

    [Fact]
    public async Task Should_SetPlainText_When_ContentIsPlainText()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var content = new TextContent("Hello World");
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);

        Assert.Equal("Hello World", message.PlainTextContent);
    }

    [Fact]
    public async Task Should_SetHtmlContent_When_ContentIsHtml()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var content = new HtmlContent("<h1>Hello</h1>");
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);

        Assert.Equal("<h1>Hello</h1>", message.HtmlContent);
    }

    [Fact]
    public async Task Should_SetMultipartContent()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var parts = new List<MessageContentPart>
        {
            new TextContentPart("Plain text"),
            new HtmlContentPart("<b>HTML</b>")
        };
        var content = new MultipartContent(parts);
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);

        Assert.Equal("Plain text", message.PlainTextContent);
        Assert.Equal("<b>HTML</b>", message.HtmlContent);
    }

    [Fact]
    public async Task Should_SetTemplateId_When_ContentIsTemplate()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var content = new TemplateContent("d-template123");
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);

        Assert.Equal("d-template123", message.TemplateId);
    }

    [Fact]
    public async Task Should_SetTemplateData_When_ContentIsTemplateWithParameters()
    {
        var builder = CreateBuilder();
        var sendGridMessage = new SendGridMessage();
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["city"] = "NYC"
        };
        var content = new TemplateContent("d-template456", parameters);
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, sendGridMessage, msg);

        Assert.Equal("d-template456", sendGridMessage.TemplateId);
    }

    [Fact]
    public async Task Should_SetPlainText_When_ContentTypeIsUnknownButIsTextContent()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var content = new TextContent("Fallback text");
        var msg = new Message
        {
            Id = "msg-1",
            Content = content,
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com")
        };

        await InvokeSetMessageContentAsync(builder, message, msg);

        Assert.Equal("Fallback text", message.PlainTextContent);
    }

    #endregion

    #region ApplyMessageSettings

    private static Dictionary<string, string> GetHeaders(SendGridMessage message)
    {
        if (message.Personalizations?.Count > 0)
            return message.Personalizations[0].Headers ?? new Dictionary<string, string>();
        return new Dictionary<string, string>();
    }

    [Fact]
    public void Should_SetHighPriorityHeader()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Priority"] = "high"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        var headers = GetHeaders(message);
        Assert.Equal("1", headers["X-Priority"]);
    }

    [Fact]
    public void Should_SetNormalPriorityHeader()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Priority"] = "normal"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        var headers = GetHeaders(message);
        Assert.Equal("3", headers["X-Priority"]);
    }

    [Fact]
    public void Should_SetLowPriorityHeader()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Priority"] = "low"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        var headers = GetHeaders(message);
        Assert.Equal("5", headers["X-Priority"]);
    }

    [Fact]
    public void Should_DefaultToNormal_When_UnknownPriority()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Priority"] = "urgent"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        var headers = GetHeaders(message);
        Assert.Equal("3", headers["X-Priority"]);
    }

    [Fact]
    public void Should_SkipPriority_When_ValueIsNull()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Priority"] = null
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        var headers = GetHeaders(message);
        Assert.Empty(headers);
    }

    [Fact]
    public void Should_SetCategories()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Categories"] = "newsletter,marketing,welcome"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.Categories);
        Assert.Equal(3, message.Categories.Count);
        Assert.Contains("newsletter", message.Categories);
        Assert.Contains("marketing", message.Categories);
        Assert.Contains("welcome", message.Categories);
    }

    [Fact]
    public void Should_SkipCategories_When_ValueIsEmpty()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["Categories"] = ""
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.Categories);
    }

    [Fact]
    public void Should_SkipCategories_When_KeyNotPresent()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>();

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.Categories);
    }

    [Fact]
    public void Should_SetCustomArgs()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["CustomArgs"] = "{\"userId\":\"123\",\"campaign\":\"abc\"}"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.CustomArgs);
        Assert.Equal("123", message.CustomArgs["userId"]);
        Assert.Equal("abc", message.CustomArgs["campaign"]);
    }

    [Fact]
    public void Should_HandleInvalidCustomArgsJson()
    {
        var logger = new Mock<ILogger>();
        var builder = CreateBuilder(logger: logger.Object);
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["CustomArgs"] = "not-valid-json{{{"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.CustomArgs);
    }

    [Fact]
    public void Should_SkipCustomArgs_When_ValueIsNull()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["CustomArgs"] = null
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.CustomArgs);
    }

    [Fact]
    public void Should_SetSendAt_When_DateTimeValue()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var sendAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var properties = new Dictionary<string, object?>
        {
            ["SendAt"] = sendAt
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.SendAt);
    }

    [Fact]
    public void Should_SetSendAt_When_StringValue()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["SendAt"] = "2026-06-01T12:00:00Z"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.SendAt);
    }

    [Fact]
    public void Should_SkipSendAt_When_ValueIsInvalid()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["SendAt"] = 12345
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.SendAt);
    }

    [Fact]
    public void Should_SetBatchId()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["BatchId"] = "batch-123"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Equal("batch-123", message.BatchId);
    }

    [Fact]
    public void Should_SkipBatchId_When_ValueIsEmpty()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["BatchId"] = ""
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.BatchId);
    }

    [Fact]
    public void Should_SetAsmGroupId()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["AsmGroupId"] = "42"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.Asm);
        Assert.Equal(42, message.Asm.GroupId);
    }

    [Fact]
    public void Should_SkipAsmGroupId_When_ValueIsNotNumeric()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["AsmGroupId"] = "abc"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.Asm);
    }

    [Fact]
    public void Should_SetIpPoolName()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["IpPoolName"] = "pool-1"
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Equal("pool-1", message.IpPoolName);
    }

    [Fact]
    public void Should_SkipIpPoolName_When_ValueIsEmpty()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>
        {
            ["IpPoolName"] = ""
        };

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.IpPoolName);
    }

    [Fact]
    public void Should_SetReplyTo_When_DefaultReplyToConfigured()
    {
        var builder = CreateBuilder(defaultReplyTo: "replies@test.com");
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>();

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.NotNull(message.ReplyTo);
        Assert.Equal("replies@test.com", message.ReplyTo.Email);
    }

    [Fact]
    public void Should_SkipReplyTo_When_DefaultReplyToNotConfigured()
    {
        var builder = CreateBuilder();
        var message = new SendGridMessage();
        var properties = new Dictionary<string, object?>();

        InvokeApplyMessageSettings(builder, message, CreateDummyMessage(), properties);

        Assert.Null(message.ReplyTo);
    }

    #endregion

    #region ApplyConnectorSettings

    [Fact]
    public void Should_EnableSandboxMode_When_Configured()
    {
        var builder = CreateBuilder(sandboxMode: true);
        var message = new SendGridMessage();

        InvokeApplyConnectorSettings(builder, message);

        Assert.NotNull(message.MailSettings);
        Assert.NotNull(message.MailSettings.SandboxMode);
        Assert.True(message.MailSettings.SandboxMode.Enable);
    }

    [Fact]
    public void Should_EnableTracking_When_Configured()
    {
        var builder = CreateBuilder(trackingSettings: true);
        var message = new SendGridMessage();

        InvokeApplyConnectorSettings(builder, message);

        Assert.NotNull(message.TrackingSettings);
        Assert.NotNull(message.TrackingSettings.ClickTracking);
        Assert.True(message.TrackingSettings.ClickTracking.Enable);
        Assert.NotNull(message.TrackingSettings.OpenTracking);
        Assert.True(message.TrackingSettings.OpenTracking.Enable);
    }

    [Fact]
    public void Should_EnableBoth_When_SandboxAndTrackingConfigured()
    {
        var builder = CreateBuilder(sandboxMode: true, trackingSettings: true);
        var message = new SendGridMessage();

        InvokeApplyConnectorSettings(builder, message);

        Assert.NotNull(message.MailSettings);
        Assert.True(message.MailSettings.SandboxMode.Enable);
        Assert.NotNull(message.TrackingSettings);
        Assert.True(message.TrackingSettings.ClickTracking.Enable);
        Assert.True(message.TrackingSettings.OpenTracking.Enable);
    }

    [Fact]
    public void Should_NotSetMailSettings_When_SandboxModeNotConfigured()
    {
        var builder = CreateBuilder(sandboxMode: false);
        var message = new SendGridMessage();

        InvokeApplyConnectorSettings(builder, message);

        Assert.Null(message.MailSettings);
    }

    [Fact]
    public void Should_NotSetTrackingSettings_When_TrackingNotConfigured()
    {
        var builder = CreateBuilder(trackingSettings: false);
        var message = new SendGridMessage();

        InvokeApplyConnectorSettings(builder, message);

        Assert.Null(message.TrackingSettings);
    }

    #endregion

    private static IMessage CreateDummyMessage()
    {
        return new Message
        {
            Id = "test-msg",
            Sender = new Endpoint(EndpointType.EmailAddress, "from@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "to@test.com"),
            Content = new TextContent("Test")
        };
    }
}
