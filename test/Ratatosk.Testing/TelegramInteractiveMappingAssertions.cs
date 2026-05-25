using Xunit;

using Telegram.Bot.Types.ReplyMarkups;

using Ratatosk;

namespace Ratatosk.Testing;

public static class TelegramInteractiveMappingAssertions {
	public static void AssertMapsToInlineKeyboard(IReadOnlyList<IButtonContent> source, InlineKeyboardMarkup target) {
		Assert.NotNull(target);
		var buttons = target.InlineKeyboard;
		Assert.NotNull(buttons);
		Assert.Equal(source.Count, buttons.Count());

		var index = 0;
		foreach (var row in buttons) {
			var sourceButton = source[index];
			var tgButton = Assert.Single(row);

			switch (sourceButton.ButtonType) {
				case ButtonType.Url:
					Assert.Equal(sourceButton.Text, tgButton.Text);
					Assert.Equal(sourceButton.Value, tgButton.Url);
					break;
				case ButtonType.Postback:
					Assert.Equal(sourceButton.Text, tgButton.Text);
					Assert.Equal(sourceButton.Value ?? sourceButton.Text, tgButton.CallbackData);
					break;
			}

			index++;
		}
	}

	public static void AssertMapsToReplyKeyboard(IReadOnlyList<IQuickReplyContent> source, ReplyKeyboardMarkup target) {
		Assert.NotNull(target);
		Assert.True(target.OneTimeKeyboard);
		Assert.True(target.ResizeKeyboard);
		var keyboard = target.Keyboard;
		Assert.NotNull(keyboard);
		Assert.Equal(source.Count, keyboard.Count());

		var index = 0;
		foreach (var row in keyboard) {
			var sourceQr = source[index];
			var tgButton = Assert.Single(row);
			Assert.Equal(sourceQr.Title, tgButton.Text);
			index++;
		}
	}
}
