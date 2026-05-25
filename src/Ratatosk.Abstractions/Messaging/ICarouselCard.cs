namespace Ratatosk {
	/// <summary>
	/// Represents a single card in a carousel of interactive elements.
	/// </summary>
	public interface ICarouselCard {
		/// <summary>
		/// Gets the title of the card.
		/// </summary>
		string? Title { get; }

		/// <summary>
		/// Gets an optional subtitle displayed below the title.
		/// </summary>
		string? Subtitle { get; }

		/// <summary>
		/// Gets an optional URL of an image to display on the card.
		/// </summary>
		string? ImageUrl { get; }

		/// <summary>
		/// Gets a list of buttons attached to the card.
		/// </summary>
		IList<IButtonContent> Buttons { get; }
	}
}
