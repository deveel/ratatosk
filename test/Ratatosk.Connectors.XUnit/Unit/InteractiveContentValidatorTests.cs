using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "InteractiveContentValidator")]
public class InteractiveContentValidatorTests {
	[Fact]
	public void Should_ReturnNull_When_ValidateButtonCountWithNonInteractiveContent() {
		var message = new Message { Content = new TextContent("Hello") };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 3);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateButtonCountWithButtonAndMaxLessThanOne() {
		var message = new Message { Content = new ButtonContent("Click", ButtonType.Postback) };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 0);

		Assert.NotNull(result);
		Assert.Contains("at least 1", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateButtonCountWithButtonAndMaxIsOne() {
		var message = new Message { Content = new ButtonContent("Click", ButtonType.Postback) };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 1);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateButtonCountCarouselCardExceedsMax() {
		var carousel = new CarouselContent();
		carousel.AddCard(new CarouselCard("Card", "Sub") {
			Buttons = {
				new ButtonContent("A", ButtonType.Postback),
				new ButtonContent("B", ButtonType.Postback),
				new ButtonContent("C", ButtonType.Postback)
			}
		});
		var message = new Message { Content = carousel };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 2);

		Assert.NotNull(result);
		Assert.Contains("exceed the maximum", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateButtonCountListPickerExceedsMax() {
		var picker = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1"),
			new ListPickerItem("Item 2"),
			new ListPickerItem("Item 3")
		});
		var message = new Message { Content = picker };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 2);

		Assert.NotNull(result);
		Assert.Contains("exceed the maximum", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateButtonCountListPickerWithinLimit() {
		var picker = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1"),
			new ListPickerItem("Item 2")
		});
		var message = new Message { Content = picker };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 3);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateButtonCountCarouselWithinLimit() {
		var carousel = new CarouselContent();
		carousel.AddCard(new CarouselCard("Card", "Sub") {
			Buttons = { new ButtonContent("A", ButtonType.Postback) }
		});
		var message = new Message { Content = carousel };

		var result = InteractiveContentValidator.ValidateButtonCount(message, 3);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateCarouselCardCountWithinLimit() {
		var carousel = new CarouselContent(new[] {
			new CarouselCard("Card 1"),
			new CarouselCard("Card 2")
		});

		var result = InteractiveContentValidator.ValidateCarouselCardCount(carousel, 5);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateCarouselCardCountExceedsLimit() {
		var carousel = new CarouselContent(new[] {
			new CarouselCard("Card 1"),
			new CarouselCard("Card 2"),
			new CarouselCard("Card 3")
		});

		var result = InteractiveContentValidator.ValidateCarouselCardCount(carousel, 2);

		Assert.NotNull(result);
		Assert.Contains("exceed the maximum", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateListItemCountWithinRange() {
		var picker = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1"),
			new ListPickerItem("Item 2")
		});

		var result = InteractiveContentValidator.ValidateListItemCount(picker, 1, 5);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateListItemCountBelowMinimum() {
		var picker = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1")
		});

		var result = InteractiveContentValidator.ValidateListItemCount(picker, 2, 5);

		Assert.NotNull(result);
		Assert.Contains("requires at least", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateListItemCountAboveMaximum() {
		var picker = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1"),
			new ListPickerItem("Item 2"),
			new ListPickerItem("Item 3")
		});

		var result = InteractiveContentValidator.ValidateListItemCount(picker, 1, 2);

		Assert.NotNull(result);
		Assert.Contains("exceed the maximum", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateButtonTypeIsAllowed() {
		var button = new ButtonContent("Click", ButtonType.Postback);

		var result = InteractiveContentValidator.ValidateButtonType(button, ButtonType.Postback, ButtonType.Url);

		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnError_When_ValidateButtonTypeIsNotAllowed() {
		var button = new ButtonContent("Click", ButtonType.PhoneNumber);

		var result = InteractiveContentValidator.ValidateButtonType(button, ButtonType.Url, ButtonType.Postback);

		Assert.NotNull(result);
		Assert.Contains("not supported", result.ErrorMessage);
	}

	[Fact]
	public void Should_ReturnNull_When_ValidateButtonTypeWithEmptyAllowed() {
		var button = new ButtonContent("Click", ButtonType.Url);

		var result = InteractiveContentValidator.ValidateButtonType(button);

		Assert.Null(result);
	}
}
