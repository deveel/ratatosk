namespace Deveel.Messaging {
	/// <summary>
	/// Represents a quick reply option that can be presented
	/// to the user as a selectable response.
	/// </summary>
	public class QuickReplyContent : MessageContent, IQuickReplyContent {
		/// <summary>
		/// Constructs the content with the given title and optional payload and image.
		/// </summary>
		/// <param name="title">The title of the quick reply option.</param>
		/// <param name="payload">An optional payload sent back when selected.</param>
		/// <param name="imageUrl">An optional URL of an image to display.</param>
		public QuickReplyContent(string title, string? payload = null, string? imageUrl = null) {
			Title = title;
			Payload = payload;
			ImageUrl = imageUrl;
		}

		/// <summary>
		/// Constructs the content by copying from the given interface instance.
		/// </summary>
		/// <param name="content">The source instance of <see cref="IQuickReplyContent"/>.</param>
		public QuickReplyContent(IQuickReplyContent content)
			: this(content?.Title!, content?.Payload, content?.ImageUrl) {
		}

		/// <summary>
		/// Initializes a new instance with default values.
		/// </summary>
		public QuickReplyContent() {
		}

		/// <inheritdoc/>
		public override MessageContentType ContentType => MessageContentType.QuickReply;

		/// <summary>
		/// Gets or sets the title of the quick reply option.
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the optional payload sent back when selected.
		/// </summary>
		public string? Payload { get; set; }

		/// <summary>
		/// Gets or sets the optional URL of an image to display.
		/// </summary>
		public string? ImageUrl { get; set; }
	}
}
