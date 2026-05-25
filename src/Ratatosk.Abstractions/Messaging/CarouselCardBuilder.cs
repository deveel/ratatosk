namespace Ratatosk {
	/// <summary>
	/// A builder for constructing <see cref="CarouselCard"/> instances
	/// using a fluent API.
	/// </summary>
	public class CarouselCardBuilder {
		private readonly List<IButtonContent> _buttons = new();

		/// <summary>
		/// Gets or sets the URL of an image displayed on the card.
		/// </summary>
		public string? ImageUrl { get; set; }

		/// <summary>
		/// Gets or sets the title of the card.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the subtitle of the card.
		/// </summary>
		public string? Subtitle { get; set; }

		/// <summary>
		/// Adds a button to the card.
		/// </summary>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="buttonType">The type of the button.</param>
		/// <param name="value">An optional value (URL, callback data, etc.).</param>
		/// <returns>This instance for chaining.</returns>
		public CarouselCardBuilder WithButton(string text, ButtonType buttonType, string? value = null) {
			_buttons.Add(new ButtonContent(text, buttonType, value));
			return this;
		}

		/// <summary>
		/// Builds a <see cref="CarouselCard"/> instance from the configured values.
		/// </summary>
		/// <returns>A new <see cref="CarouselCard"/>.</returns>
		public CarouselCard Build() {
			return new CarouselCard(Title, Subtitle, ImageUrl) {
				Buttons = _buttons
			};
		}
	}
}
