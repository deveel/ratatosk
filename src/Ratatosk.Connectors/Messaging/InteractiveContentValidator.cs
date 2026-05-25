using System.ComponentModel.DataAnnotations;

namespace Ratatosk {
	/// <summary>
	/// Provides validation logic for interactive message content types
	/// such as buttons, carousels, and list pickers.
	/// </summary>
	public static class InteractiveContentValidator {
		/// <summary>
		/// Validates that the number of buttons in a message does not
		/// exceed the maximum allowed by the channel.
		/// </summary>
		/// <param name="message">The message to validate.</param>
		/// <param name="maxButtons">The maximum number of buttons allowed.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> if validation fails; otherwise <c>null</c>.
		/// </returns>
		public static ValidationResult? ValidateButtonCount(IMessage message, int maxButtons) {
			if (message.Content is IButtonContent && maxButtons < 1)
				return new ValidationResult($"Button content requires at least 1 button, but the maximum is {maxButtons}", new[] { "Content" });

			if (message.Content is ICarouselContent carousel) {
				foreach (var card in carousel.Cards) {
					if (card.Buttons.Count > maxButtons)
						return new ValidationResult($"Carousel card buttons exceed the maximum of {maxButtons}", new[] { "Content.Cards" });
				}
			}

			if (message.Content is IListPickerContent listPicker && listPicker.Items.Count > maxButtons)
				return new ValidationResult($"List picker items exceed the maximum of {maxButtons}", new[] { "Content.Items" });

			return null;
		}

		/// <summary>
		/// Validates that the number of cards in a carousel does not
		/// exceed the maximum allowed by the channel.
		/// </summary>
		/// <param name="carousel">The carousel content to validate.</param>
		/// <param name="maxCards">The maximum number of cards allowed.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> if validation fails; otherwise <c>null</c>.
		/// </returns>
		public static ValidationResult? ValidateCarouselCardCount(ICarouselContent carousel, int maxCards) {
			if (carousel.Cards.Count > maxCards)
				return new ValidationResult($"Carousel cards exceed the maximum of {maxCards}", new[] { "Content.Cards" });

			return null;
		}

		/// <summary>
		/// Validates that the number of items in a list picker is within
		/// the allowed range.
		/// </summary>
		/// <param name="listPicker">The list picker content to validate.</param>
		/// <param name="minItems">The minimum number of items required.</param>
		/// <param name="maxItems">The maximum number of items allowed.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> if validation fails; otherwise <c>null</c>.
		/// </returns>
		public static ValidationResult? ValidateListItemCount(IListPickerContent listPicker, int minItems, int maxItems) {
			if (listPicker.Items.Count < minItems)
				return new ValidationResult($"List picker requires at least {minItems} items, but found {listPicker.Items.Count}", new[] { "Content.Items" });

			if (listPicker.Items.Count > maxItems)
				return new ValidationResult($"List picker items exceed the maximum of {maxItems}", new[] { "Content.Items" });

			return null;
		}

		/// <summary>
		/// Validates that the button type is within the set of types
		/// allowed by the channel.
		/// </summary>
		/// <param name="button">The button content to validate.</param>
		/// <param name="allowedTypes">The set of allowed button types.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> if validation fails; otherwise <c>null</c>.
		/// </returns>
		public static ValidationResult? ValidateButtonType(IButtonContent button, params ButtonType[] allowedTypes) {
			if (allowedTypes.Length > 0 && !allowedTypes.Contains(button.ButtonType))
				return new ValidationResult($"Button type '{button.ButtonType}' is not supported. Allowed types: {string.Join(", ", allowedTypes)}", new[] { "Content.ButtonType" });

			return null;
		}
	}
}
