namespace Ratatosk {
	/// <summary>
	/// Represents a single card within a carousel content.
	/// </summary>
	public class CarouselCard : ICarouselCard {
		/// <summary>
		/// Constructs the card with the given title, subtitle, and image URL.
		/// </summary>
		/// <param name="title">The title of the card.</param>
		/// <param name="subtitle">An optional subtitle.</param>
		/// <param name="imageUrl">An optional URL of an image.</param>
		public CarouselCard(string? title = null, string? subtitle = null, string? imageUrl = null) {
			Title = title;
			Subtitle = subtitle;
			ImageUrl = imageUrl;
		}

		/// <summary>
		/// Constructs the card by copying from the given interface instance.
		/// </summary>
		/// <param name="card">The source instance of <see cref="ICarouselCard"/>.</param>
		public CarouselCard(ICarouselCard card) {
			Title = card.Title;
			Subtitle = card.Subtitle;
			ImageUrl = card.ImageUrl;
			foreach (var button in card.Buttons)
				Buttons.Add(new ButtonContent(button));
		}

		/// <summary>
		/// Initializes a new instance with default values.
		/// </summary>
		public CarouselCard() {
		}

		/// <summary>
		/// Gets or sets the title of the card.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the subtitle of the card.
		/// </summary>
		public string? Subtitle { get; set; }

		/// <summary>
		/// Gets or sets the URL of an image displayed on the card.
		/// </summary>
		public string? ImageUrl { get; set; }

		/// <summary>
		/// Gets or sets the list of buttons attached to the card.
		/// </summary>
		public IList<IButtonContent> Buttons { get; set; } = new List<IButtonContent>();
	}
}
