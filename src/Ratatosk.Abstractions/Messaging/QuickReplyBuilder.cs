namespace Ratatosk {
	/// <summary>
	/// A builder for constructing <see cref="QuickReplyContent"/> instances
	/// using a fluent API.
	/// </summary>
	public class QuickReplyBuilder {
		/// <summary>
		/// Gets or sets the title of the quick reply option.
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the optional payload returned when selected.
		/// </summary>
		public string? Payload { get; set; }

		/// <summary>
		/// Gets or sets the optional URL of an image to display.
		/// </summary>
		public string? ImageUrl { get; set; }

		/// <summary>
		/// Sets the title of the quick reply option.
		/// </summary>
		/// <param name="title">The title text.</param>
		/// <returns>This instance for chaining.</returns>
		public QuickReplyBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		/// <summary>
		/// Sets the payload returned when the quick reply is selected.
		/// </summary>
		/// <param name="payload">The payload string.</param>
		/// <returns>This instance for chaining.</returns>
		public QuickReplyBuilder WithPayload(string? payload) {
			Payload = payload;
			return this;
		}

		/// <summary>
		/// Sets the URL of an image to display alongside the quick reply.
		/// </summary>
		/// <param name="imageUrl">The image URL.</param>
		/// <returns>This instance for chaining.</returns>
		public QuickReplyBuilder WithImageUrl(string? imageUrl) {
			ImageUrl = imageUrl;
			return this;
		}

		/// <summary>
		/// Builds a <see cref="QuickReplyContent"/> instance from the configured values.
		/// </summary>
		/// <returns>A new <see cref="QuickReplyContent"/>.</returns>
		public QuickReplyContent Build() => new(Title, Payload, ImageUrl);
	}
}
