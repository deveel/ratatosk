namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ButtonContent")]
public class ButtonContentTests {
	[Fact]
	public void Should_SetProperties_When_ButtonContentConstructor() {
		var content = new ButtonContent("Click Me", ButtonType.Url, "https://example.com");

		Assert.Equal("Click Me", content.Text);
		Assert.Equal(ButtonType.Url, content.ButtonType);
		Assert.Equal("https://example.com", content.Value);
		Assert.Equal(MessageContentType.Button, content.ContentType);
	}

	[Fact]
	public void Should_SetPostbackButton_When_ConstructorWithPostback() {
		var content = new ButtonContent("Buy Now", ButtonType.Postback, "BUY_PRODUCT");

		Assert.Equal("Buy Now", content.Text);
		Assert.Equal(ButtonType.Postback, content.ButtonType);
		Assert.Equal("BUY_PRODUCT", content.Value);
	}

	[Fact]
	public void Should_SetPhoneNumberButton_When_ConstructorWithPhoneNumber() {
		var content = new ButtonContent("Call Us", ButtonType.PhoneNumber, "+1234567890");

		Assert.Equal("Call Us", content.Text);
		Assert.Equal(ButtonType.PhoneNumber, content.ButtonType);
		Assert.Equal("+1234567890", content.Value);
	}

	[Fact]
	public void Should_CopyProperties_When_ButtonContentCopyConstructor() {
		var source = new ButtonContent("Click Me", ButtonType.Postback, "PAYLOAD");
		var copy = new ButtonContent(source);

		Assert.Equal(source.Text, copy.Text);
		Assert.Equal(source.ButtonType, copy.ButtonType);
		Assert.Equal(source.Value, copy.Value);
	}

	[Fact]
	public void Should_UseDefaults_When_ParameterlessConstructor() {
		var content = new ButtonContent();

		Assert.Equal(string.Empty, content.Text);
		Assert.Equal(default(ButtonType), content.ButtonType);
		Assert.Null(content.Value);
	}

	[Fact]
	public void Should_AcceptNullValue_When_ButtonContentConstructor() {
		var content = new ButtonContent("Click Me", ButtonType.Postback);

		Assert.Equal("Click Me", content.Text);
		Assert.Equal(ButtonType.Postback, content.ButtonType);
		Assert.Null(content.Value);
	}

	[Fact]
	public void Should_UpdateValue_When_ButtonContentPropertySetter() {
		var content = new ButtonContent("Click", ButtonType.Url);
		content.Text = "Updated";
		content.ButtonType = ButtonType.Postback;
		content.Value = "NEW_PAYLOAD";

		Assert.Equal("Updated", content.Text);
		Assert.Equal(ButtonType.Postback, content.ButtonType);
		Assert.Equal("NEW_PAYLOAD", content.Value);
	}

	[Fact]
	public void Should_ExposeCorrectType_When_IButtonContentInterface() {
		IButtonContent content = new ButtonContent("Test", ButtonType.Url, "https://test.com");

		Assert.Equal("Test", content.Text);
		Assert.Equal(ButtonType.Url, content.ButtonType);
		Assert.Equal("https://test.com", content.Value);
	}

	[Fact]
	public void Should_BeInteractive_When_IInteractiveContentInterface() {
		IInteractiveContent content = new ButtonContent("Test", ButtonType.Postback, "p");
		Assert.Equal(MessageContentType.Button, content.ContentType);
	}
}
