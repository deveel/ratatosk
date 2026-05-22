namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "QuickReplyBuilder")]
public class QuickReplyBuilderTests {
	[Fact]
	public void Should_SetProperties_When_FluentMethodsCalled() {
		var qr = new QuickReplyBuilder()
			.WithTitle("Yes")
			.WithPayload("YES_PAYLOAD")
			.WithImageUrl("https://img.url")
			.Build();

		Assert.Equal("Yes", qr.Title);
		Assert.Equal("YES_PAYLOAD", qr.Payload);
		Assert.Equal("https://img.url", qr.ImageUrl);
	}

	[Fact]
	public void Should_BuildWithDefaults_When_NoValuesSet() {
		var qr = new QuickReplyBuilder().Build();

		Assert.Equal(string.Empty, qr.Title);
		Assert.Null(qr.Payload);
		Assert.Null(qr.ImageUrl);
	}

	[Fact]
	public void Should_ProduceQuickReplyContent_When_BuildCalled() {
		var result = new QuickReplyBuilder()
			.WithTitle("Test")
			.Build();

		Assert.IsType<QuickReplyContent>(result);
		Assert.Equal(MessageContentType.QuickReply, result.ContentType);
	}

	[Fact]
	public void Should_SetProperties_When_PropertiesAssignedDirectly() {
		var builder = new QuickReplyBuilder {
			Title = "Maybe",
			Payload = "MAYBE_PAYLOAD",
			ImageUrl = "https://img.url"
		};

		Assert.Equal("Maybe", builder.Title);
		Assert.Equal("MAYBE_PAYLOAD", builder.Payload);
		Assert.Equal("https://img.url", builder.ImageUrl);
	}
}
