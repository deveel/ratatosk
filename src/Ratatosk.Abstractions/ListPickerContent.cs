namespace Ratatosk {
	/// <summary>
	/// Represents a list picker content that allows the user
	/// to select from a vertical list of items.
	/// </summary>
	public class ListPickerContent : MessageContent, IListPickerContent {
		/// <summary>
		/// Constructs the content with the given title, subtitle, items, and style.
		/// </summary>
		/// <param name="title">The title of the list picker.</param>
		/// <param name="subtitle">An optional subtitle.</param>
		/// <param name="items">An optional collection of items.</param>
		/// <param name="style">The display style of the list.</param>
		public ListPickerContent(string? title = null, string? subtitle = null, IEnumerable<IListPickerItem>? items = null, ListPickerStyle style = ListPickerStyle.Inlined) {
			Title = title;
			Subtitle = subtitle;
			Style = style;
			if (items != null) {
				_items = new List<IListPickerItem>(items);
			}
		}

		/// <summary>
		/// Constructs the content by copying from the given interface instance.
		/// </summary>
		/// <param name="content">The source instance of <see cref="IListPickerContent"/>.</param>
		public ListPickerContent(IListPickerContent content)
			: this(content?.Title, content?.Subtitle, content?.Items, content?.Style ?? ListPickerStyle.Inlined) {
		}

		/// <summary>
		/// Initializes a new empty instance.
		/// </summary>
		public ListPickerContent() {
		}

		/// <inheritdoc/>
		public override MessageContentType ContentType => MessageContentType.ListPicker;

		private readonly List<IListPickerItem> _items = new();

		/// <summary>
		/// Gets or sets the title of the list picker.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the subtitle displayed above the list.
		/// </summary>
		public string? Subtitle { get; set; }

		/// <summary>
		/// Gets a read-only list of selectable items.
		/// </summary>
		public IReadOnlyList<IListPickerItem> Items => _items.AsReadOnly();

		/// <summary>
		/// Gets or sets the display style of the list.
		/// </summary>
		public ListPickerStyle Style { get; set; }

		/// <summary>
		/// Adds an item to the list picker.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="item"/> is <c>null</c>.
		/// </exception>
		public void AddItem(IListPickerItem item) {
			ArgumentNullException.ThrowIfNull(item, nameof(item));
			_items.Add(item);
		}

		/// <summary>
		/// Removes an item from the list picker.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>
		/// <c>true</c> if the item was removed; otherwise <c>false</c>.
		/// </returns>
		public bool RemoveItem(IListPickerItem item) {
			return _items.Remove(item);
		}

		/// <summary>
		/// Removes all items from the list picker.
		/// </summary>
		public void ClearItems() {
			_items.Clear();
		}
	}
}
