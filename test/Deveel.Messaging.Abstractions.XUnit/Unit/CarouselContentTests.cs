namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "CarouselContent")]
public class CarouselContentTests {
	[Fact]
	public void Should_CreateCarousel_When_ConstructorWithCards() {
		var cards = new List<ICarouselCard> {
			new CarouselCard("Card 1", "Sub 1", "https://img1.url"),
			new CarouselCard("Card 2", "Sub 2", "https://img2.url")
		};
		var content = new CarouselContent(cards);

		Assert.Equal(2, content.Cards.Count);
		Assert.Equal(MessageContentType.Carousel, content.ContentType);
	}

	[Fact]
	public void Should_AddCard_When_AddCardCalled() {
		var content = new CarouselContent();
		content.AddCard(new CarouselCard("Card", "Sub", "img"));

		Assert.Single(content.Cards);
	}

	[Fact]
	public void Should_RemoveCard_When_RemoveCardCalled() {
		var card = new CarouselCard("Card", "Sub", "img");
		var content = new CarouselContent(new[] { card });

		var result = content.RemoveCard(card);

		Assert.True(result);
		Assert.Empty(content.Cards);
	}

	[Fact]
	public void Should_ClearCards_When_ClearCardsCalled() {
		var content = new CarouselContent(new[] {
			new CarouselCard("Card 1"),
			new CarouselCard("Card 2")
		});
		content.ClearCards();

		Assert.Empty(content.Cards);
	}

	[Fact]
	public void Should_CopyCards_When_CopyConstructor() {
		var source = new CarouselContent(new[] {
			new CarouselCard("Card 1", "Sub", "img") { Buttons = { new ButtonContent("Btn", ButtonType.Url, "url") } }
		});
		var copy = new CarouselContent(source);

		Assert.Equal(source.Cards.Count, copy.Cards.Count);
		Assert.Equal(source.Cards[0].Title, copy.Cards[0].Title);
	}
}

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "CarouselCard")]
public class CarouselCardTests {
	[Fact]
	public void Should_SetProperties_When_CarouselCardConstructor() {
		var card = new CarouselCard("Title", "Subtitle", "https://img.url");

		Assert.Equal("Title", card.Title);
		Assert.Equal("Subtitle", card.Subtitle);
		Assert.Equal("https://img.url", card.ImageUrl);
		Assert.Empty(card.Buttons);
	}

	[Fact]
	public void Should_AddButton_When_ButtonsListModified() {
		var card = new CarouselCard();
		card.Buttons.Add(new ButtonContent("Click", ButtonType.Url, "url"));

		Assert.Single(card.Buttons);
	}

	[Fact]
	public void Should_CopyProperties_When_CarouselCardCopyConstructor() {
		var source = new CarouselCard("Title", "Sub", "img");
		source.Buttons.Add(new ButtonContent("Btn", ButtonType.Postback, "p"));

		var copy = new CarouselCard(source);

		Assert.Equal(source.Title, copy.Title);
		Assert.Equal(source.Subtitle, copy.Subtitle);
		Assert.Equal(source.ImageUrl, copy.ImageUrl);
		Assert.Equal(source.Buttons.Count, copy.Buttons.Count);
	}
}
