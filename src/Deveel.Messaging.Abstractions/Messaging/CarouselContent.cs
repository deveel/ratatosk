namespace Deveel.Messaging {
	/// <summary>
	/// Represents a carousel of interactive cards that can
	/// be scrolled through by the user.
	/// </summary>
	public class CarouselContent : MessageContent, ICarouselContent {
		/// <summary>
		/// Constructs the content with a collection of cards.
		/// </summary>
		/// <param name="cards">The collection of cards to include.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="cards"/> is <c>null</c>.
		/// </exception>
		public CarouselContent(IEnumerable<ICarouselCard> cards) {
			ArgumentNullException.ThrowIfNull(cards, nameof(cards));
			_cards = new List<ICarouselCard>(cards);
		}

		/// <summary>
		/// Constructs the content by copying from the given interface instance.
		/// </summary>
		/// <param name="content">The source instance of <see cref="ICarouselContent"/>.</param>
		public CarouselContent(ICarouselContent content)
			: this(content?.Cards ?? Enumerable.Empty<ICarouselCard>()) {
		}

		/// <summary>
		/// Initializes a new empty instance.
		/// </summary>
		public CarouselContent() {
		}

		/// <inheritdoc/>
		public override MessageContentType ContentType => MessageContentType.Carousel;

		private readonly List<ICarouselCard> _cards = new();

		/// <summary>
		/// Gets a read-only list of cards in the carousel.
		/// </summary>
		public IReadOnlyList<ICarouselCard> Cards => _cards.AsReadOnly();

		/// <summary>
		/// Adds a card to the carousel.
		/// </summary>
		/// <param name="card">The card to add.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="card"/> is <c>null</c>.
		/// </exception>
		public void AddCard(ICarouselCard card) {
			ArgumentNullException.ThrowIfNull(card, nameof(card));
			_cards.Add(card);
		}

		/// <summary>
		/// Removes a card from the carousel.
		/// </summary>
		/// <param name="card">The card to remove.</param>
		/// <returns>
		/// <c>true</c> if the card was removed; otherwise <c>false</c>.
		/// </returns>
		public bool RemoveCard(ICarouselCard card) {
			return _cards.Remove(card);
		}

		/// <summary>
		/// Removes all cards from the carousel.
		/// </summary>
		public void ClearCards() {
			_cards.Clear();
		}
	}
}
