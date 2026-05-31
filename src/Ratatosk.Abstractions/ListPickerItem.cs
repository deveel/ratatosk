namespace Ratatosk {
	/// <summary>
	/// Represents a single item in a list picker selection.
	/// </summary>
	public class ListPickerItem : IListPickerItem {
		/// <summary>
		/// Constructs the item with the given title and optional details.
		/// </summary>
		/// <param name="title">The title of the item.</param>
		/// <param name="description">An optional description.</param>
		/// <param name="imageUrl">An optional URL of an image.</param>
		/// <param name="payload">An optional payload returned when selected.</param>
		public ListPickerItem(string title, string? description = null, string? imageUrl = null, string? payload = null) {
			Title = title;
			Description = description;
			ImageUrl = imageUrl;
			Payload = payload;
		}

		/// <summary>
		/// Constructs the item by copying from the given interface instance.
		/// </summary>
		/// <param name="item">The source instance of <see cref="IListPickerItem"/>.</param>
		public ListPickerItem(IListPickerItem item)
			: this(item?.Title!, item?.Description, item?.ImageUrl, item?.Payload) {
		}

		/// <summary>
		/// Initializes a new instance with default values.
		/// </summary>
		public ListPickerItem() {
		}

		/// <summary>
		/// Gets or sets the title of the list item.
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the optional description of the item.
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Gets or sets the optional URL of an image for the item.
		/// </summary>
		public string? ImageUrl { get; set; }

		/// <summary>
		/// Gets or sets the optional payload returned when the item is selected.
		/// </summary>
		public string? Payload { get; set; }
	}
}
