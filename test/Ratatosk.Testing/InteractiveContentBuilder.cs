using Bogus;

using Ratatosk;

namespace Ratatosk.Testing;

public static class InteractiveContentBuilder {
	private static readonly ButtonType[] SupportedButtonTypes = { ButtonType.Url, ButtonType.Postback };

	public static ButtonContent CreateButton(Action<ButtonContent>? configure = null) {
		var faker = new Faker("en") { Random = new Randomizer(0x5EED) };
		var button = new ButtonContent(
			faker.Lorem.Word(),
			faker.PickRandom(SupportedButtonTypes),
			faker.Internet.Url()
		);
		configure?.Invoke(button);
		return button;
	}

	public static List<ButtonContent> CreateButtons(int count, Action<ButtonContent>? configure = null) {
		return Enumerable.Range(0, count)
			.Select(_ => CreateButton(configure))
			.ToList();
	}

	public static QuickReplyContent CreateQuickReply(Action<QuickReplyContent>? configure = null) {
		var faker = new Faker("en") { Random = new Randomizer(0x5EED) };
		var qr = new QuickReplyContent(
			faker.Lorem.Word(),
			faker.Lorem.Sentence(),
			faker.Internet.Url()
		);
		configure?.Invoke(qr);
		return qr;
	}

	public static List<QuickReplyContent> CreateQuickReplies(int count, Action<QuickReplyContent>? configure = null) {
		return Enumerable.Range(0, count)
			.Select(_ => CreateQuickReply(configure))
			.ToList();
	}

	public static CarouselContent CreateCarousel(int cardCount, Action<CarouselContent>? configure = null) {
		var faker = new Faker("en") { Random = new Randomizer(0x5EED) };
		var carousel = new CarouselContent();
		for (int i = 0; i < cardCount; i++) {
			carousel.AddCard(new CarouselCard(
				faker.Lorem.Sentence(),
				faker.Lorem.Sentence(),
				faker.Internet.Url()
			));
		}
		configure?.Invoke(carousel);
		return carousel;
	}

	public static ListPickerContent CreateListPicker(int itemCount, Action<ListPickerContent>? configure = null) {
		var faker = new Faker("en") { Random = new Randomizer(0x5EED) };
		var picker = new ListPickerContent(
			faker.Lorem.Sentence(),
			faker.Lorem.Sentence(),
			style: faker.PickRandom<ListPickerStyle>()
		);
		for (int i = 0; i < itemCount; i++) {
			picker.AddItem(new ListPickerItem(
				faker.Lorem.Word(),
				faker.Lorem.Sentence(),
				faker.Internet.Url(),
				faker.Lorem.Sentence()
			));
		}
		configure?.Invoke(picker);
		return picker;
	}
}
