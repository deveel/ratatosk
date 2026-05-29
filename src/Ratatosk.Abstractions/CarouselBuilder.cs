namespace Ratatosk {
	/// <summary>
	/// A builder for constructing <see cref="CarouselContent"/> instances
	/// using a fluent API with sub-builders for individual cards.
	/// </summary>
	public class CarouselBuilder {
		private readonly List<CarouselCard> _cards = new();

		/// <summary>
		/// Adds a card to the carousel, configured by a sub-builder.
		/// </summary>
		/// <param name="configure">An action to configure the card.</param>
		/// <returns>This instance for chaining.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="configure"/> is <c>null</c>.
		/// </exception>
		public CarouselBuilder AddCard(Action<CarouselCardBuilder> configure) {
			ArgumentNullException.ThrowIfNull(configure, nameof(configure));

			var cardBuilder = new CarouselCardBuilder();
			configure(cardBuilder);
			_cards.Add(cardBuilder.Build());
			return this;
		}

		/// <summary>
		/// Adds a card to the carousel with the given properties
		/// and an optional sub-builder for buttons.
		/// </summary>
		/// <param name="imageUrl">The URL of an image to display on the card.</param>
		/// <param name="title">The title of the card.</param>
		/// <param name="subtitle">An optional subtitle.</param>
		/// <param name="configure">
		/// An optional action to configure the card (e.g., add buttons).
		/// </param>
		/// <returns>This instance for chaining.</returns>
		public CarouselBuilder AddCard(string? imageUrl, string? title, string? subtitle = null, Action<CarouselCardBuilder>? configure = null) {
			var cardBuilder = new CarouselCardBuilder {
				ImageUrl = imageUrl,
				Title = title,
				Subtitle = subtitle
			};
			configure?.Invoke(cardBuilder);
			_cards.Add(cardBuilder.Build());
			return this;
		}

		/// <summary>
		/// Builds a <see cref="CarouselContent"/> instance from the configured cards.
		/// </summary>
		/// <returns>A new <see cref="CarouselContent"/>.</returns>
		public CarouselContent Build() => new(_cards);
	}
}
