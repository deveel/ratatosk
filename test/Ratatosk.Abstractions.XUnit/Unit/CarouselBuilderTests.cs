namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "CarouselBuilder")]
public class CarouselBuilderTests {
	[Fact]
	public void Should_BuildCarousel_When_AddCardWithConfigure() {
		var carousel = new CarouselBuilder()
			.AddCard(card => card
				.WithButton("Buy", ButtonType.Postback, "BUY"))
			.Build();

		Assert.Single(carousel.Cards);
		Assert.Single(carousel.Cards[0].Buttons);
		Assert.Equal("Buy", carousel.Cards[0].Buttons[0].Text);
	}

	[Fact]
	public void Should_BuildCarousel_When_AddCardWithArgsAndConfigure() {
		var carousel = new CarouselBuilder()
			.AddCard("https://img.url", "Title", "Sub", card =>
				card.WithButton("Go", ButtonType.Url, "https://go.url"))
			.Build();

		Assert.Single(carousel.Cards);
		Assert.Equal("Title", carousel.Cards[0].Title);
		Assert.Equal("Sub", carousel.Cards[0].Subtitle);
		Assert.Equal("https://img.url", carousel.Cards[0].ImageUrl);
		Assert.Equal("Go", carousel.Cards[0].Buttons[0].Text);
	}

	[Fact]
	public void Should_BuildCarousel_When_AddCardWithArgsNoConfigure() {
		var carousel = new CarouselBuilder()
			.AddCard("https://img.url", "Title", "Sub")
			.Build();

		Assert.Single(carousel.Cards);
		Assert.Empty(carousel.Cards[0].Buttons);
	}

	[Fact]
	public void Should_AddMultipleCards_When_AddCardCalledMultipleTimes() {
		var carousel = new CarouselBuilder()
			.AddCard("https://img1.url", "A", "Sub A")
			.AddCard("https://img2.url", "B", "Sub B")
			.Build();

		Assert.Equal(2, carousel.Cards.Count);
		Assert.Equal("A", carousel.Cards[0].Title);
		Assert.Equal("B", carousel.Cards[1].Title);
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_AddCardWithNullConfigure() {
		var builder = new CarouselBuilder();

		Assert.Throws<ArgumentNullException>(() => builder.AddCard(null!));
	}

	[Fact]
	public void Should_ProduceCarouselContent_When_BuildCalled() {
		var result = new CarouselBuilder()
			.AddCard("img", "Title")
			.Build();

		Assert.IsType<CarouselContent>(result);
		Assert.Equal(MessageContentType.Carousel, result.ContentType);
	}
}

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "CarouselCardBuilder")]
public class CarouselCardBuilderTests {
	[Fact]
	public void Should_SetProperties_When_PropertiesAssigned() {
		var card = new CarouselCardBuilder {
			ImageUrl = "https://img.url",
			Title = "Title",
			Subtitle = "Sub"
		}.Build();

		Assert.Equal("https://img.url", card.ImageUrl);
		Assert.Equal("Title", card.Title);
		Assert.Equal("Sub", card.Subtitle);
	}

	[Fact]
	public void Should_AddButton_When_WithButtonCalled() {
		var card = new CarouselCardBuilder()
			.WithButton("Click", ButtonType.Url, "https://click.url")
			.Build();

		Assert.Single(card.Buttons);
		Assert.Equal("Click", card.Buttons[0].Text);
		Assert.Equal(ButtonType.Url, card.Buttons[0].ButtonType);
	}

	[Fact]
	public void Should_AddMultipleButtons_When_WithButtonCalledMultipleTimes() {
		var card = new CarouselCardBuilder()
			.WithButton("A", ButtonType.Postback, "A_P")
			.WithButton("B", ButtonType.Url, "https://b.url")
			.Build();

		Assert.Equal(2, card.Buttons.Count);
	}

	[Fact]
	public void Should_BuildCarouselCard_When_BuildCalled() {
		var result = new CarouselCardBuilder()
			.WithButton("Test", ButtonType.Postback, "P")
			.Build();

		Assert.IsType<CarouselCard>(result);
	}
}
