namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "QuickReplyContent")]
public class QuickReplyContentTests {
	[Fact]
	public void Should_SetProperties_When_QuickReplyContentConstructor() {
		var content = new QuickReplyContent("Yes", "YES_PAYLOAD", "https://example.com/icon.png");

		Assert.Equal("Yes", content.Title);
		Assert.Equal("YES_PAYLOAD", content.Payload);
		Assert.Equal("https://example.com/icon.png", content.ImageUrl);
		Assert.Equal(MessageContentType.QuickReply, content.ContentType);
	}

	[Fact]
	public void Should_UseDefaults_When_ParameterlessConstructor() {
		var content = new QuickReplyContent();

		Assert.Equal(string.Empty, content.Title);
		Assert.Null(content.Payload);
		Assert.Null(content.ImageUrl);
	}

	[Fact]
	public void Should_SetTitleOnly_When_QuickReplyContentConstructorWithoutPayload() {
		var content = new QuickReplyContent("No");

		Assert.Equal("No", content.Title);
		Assert.Null(content.Payload);
		Assert.Null(content.ImageUrl);
	}

	[Fact]
	public void Should_CopyProperties_When_QuickReplyContentCopyConstructor() {
		var source = new QuickReplyContent("Maybe", "MAYBE_PAYLOAD", "https://img.url");
		var copy = new QuickReplyContent(source);

		Assert.Equal(source.Title, copy.Title);
		Assert.Equal(source.Payload, copy.Payload);
		Assert.Equal(source.ImageUrl, copy.ImageUrl);
	}

	[Fact]
	public void Should_UpdateValues_When_QuickReplyContentPropertySetters() {
		var content = new QuickReplyContent("Initial");
		content.Title = "Updated";
		content.Payload = "UPDATED_PAYLOAD";
		content.ImageUrl = "https://new.img";

		Assert.Equal("Updated", content.Title);
		Assert.Equal("UPDATED_PAYLOAD", content.Payload);
		Assert.Equal("https://new.img", content.ImageUrl);
	}

	[Fact]
	public void Should_ExposeCorrectType_When_IQuickReplyContentInterface() {
		IQuickReplyContent content = new QuickReplyContent("Test", "p", "img");

		Assert.Equal("Test", content.Title);
		Assert.Equal("p", content.Payload);
		Assert.Equal("img", content.ImageUrl);
	}

	[Fact]
	public void Should_BeInteractive_When_IInteractiveContentInterface() {
		IInteractiveContent content = new QuickReplyContent("Test");
		Assert.Equal(MessageContentType.QuickReply, content.ContentType);
	}
}
