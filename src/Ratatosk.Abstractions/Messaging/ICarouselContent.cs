namespace Ratatosk {
	/// <summary>
	/// Represents a carousel of interactive cards that can
	/// be scrolled through by the user.
	/// </summary>
	public interface ICarouselContent : IInteractiveContent {
		/// <summary>
		/// Gets a read-only list of cards in the carousel.
		/// </summary>
		IReadOnlyList<ICarouselCard> Cards { get; }
	}
}
