namespace Deveel.Messaging {
	/// <summary>
	/// A builder for constructing <see cref="ListPickerContent"/> instances
	/// using a fluent API with sub-builders for individual items.
	/// </summary>
	public class ListPickerBuilder {
		private readonly List<ListPickerItem> _items = new();
		private ListPickerStyle _style = ListPickerStyle.Inlined;

		/// <summary>
		/// Gets or sets the title of the list picker.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the subtitle displayed above the list.
		/// </summary>
		public string? Subtitle { get; set; }

		/// <summary>
		/// Gets or sets the display style of the list picker.
		/// </summary>
		public ListPickerStyle Style {
			get => _style;
			set => _style = value;
		}

		/// <summary>
		/// Sets the display style of the list picker.
		/// </summary>
		/// <param name="style">The style value.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerBuilder WithStyle(ListPickerStyle style) {
			_style = style;
			return this;
		}

		/// <summary>
		/// Adds an item to the list picker with the given properties.
		/// </summary>
		/// <param name="title">The title of the item.</param>
		/// <param name="description">An optional description.</param>
		/// <param name="imageUrl">An optional image URL.</param>
		/// <param name="payload">An optional payload returned when selected.</param>
		/// <returns>This instance for chaining.</returns>
		public ListPickerBuilder AddItem(string title, string? description = null, string? imageUrl = null, string? payload = null) {
			_items.Add(new ListPickerItem(title, description, imageUrl, payload));
			return this;
		}

		/// <summary>
		/// Adds an item to the list picker, configured by a sub-builder.
		/// </summary>
		/// <param name="configure">An action to configure the item.</param>
		/// <returns>This instance for chaining.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="configure"/> is <c>null</c>.
		/// </exception>
		public ListPickerBuilder AddItem(Action<ListPickerItemBuilder> configure) {
			ArgumentNullException.ThrowIfNull(configure, nameof(configure));

			var itemBuilder = new ListPickerItemBuilder();
			configure(itemBuilder);
			_items.Add(itemBuilder.Build());
			return this;
		}

		/// <summary>
		/// Builds a <see cref="ListPickerContent"/> instance from the configured values.
		/// </summary>
		/// <returns>A new <see cref="ListPickerContent"/>.</returns>
		public ListPickerContent Build()
			=> new ListPickerContent(Title, Subtitle, _items, _style);
	}
}
