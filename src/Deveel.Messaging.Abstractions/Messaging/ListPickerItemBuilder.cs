namespace Deveel.Messaging {
	/// <summary>
	/// A builder for constructing <see cref="ListPickerItem"/> instances
	/// using a fluent API.
	/// </summary>
	public class ListPickerItemBuilder {
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

		/// <summary>
		/// Sets the title of the list item.
		/// </summary>
		/// <param name="title">The title text.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerItemBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		/// <summary>
		/// Sets the description of the list item.
		/// </summary>
		/// <param name="description">The description text.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerItemBuilder WithDescription(string? description) {
			Description = description;
			return this;
		}

		/// <summary>
		/// Sets the URL of an image for the list item.
		/// </summary>
		/// <param name="imageUrl">The image URL.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerItemBuilder WithImageUrl(string? imageUrl) {
			ImageUrl = imageUrl;
			return this;
		}

		/// <summary>
		/// Sets the payload returned when the item is selected.
		/// </summary>
		/// <param name="payload">The payload string.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerItemBuilder WithPayload(string? payload) {
			Payload = payload;
			return this;
		}

		/// <summary>
		/// Builds a <see cref="ListPickerItem"/> instance from the configured values.
		/// </summary>
		/// <returns>A new <see cref="ListPickerItem"/>.</returns>
		public ListPickerItem Build() => new(Title, Description, ImageUrl, Payload);
	}
}
