namespace Deveel.Messaging {
	/// <summary>
	/// Represents a clickable button that can be attached to a message.
	/// </summary>
	public class ButtonContent : MessageContent, IButtonContent {
		/// <summary>
		/// Constructs the content with the given text, type, and optional value.
		/// </summary>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="buttonType">The type of the button.</param>
		/// <param name="value">An optional value such as a URL or callback payload.</param>
		public ButtonContent(string text, ButtonType buttonType, string? value = null) {
			Text = text;
			ButtonType = buttonType;
			Value = value;
		}

		/// <summary>
		/// Constructs the content by copying from the given interface instance.
		/// </summary>
		/// <param name="content">The source instance of <see cref="IButtonContent"/>.</param>
		public ButtonContent(IButtonContent content)
			: this(content?.Text!, content?.ButtonType ?? default, content?.Value) {
		}

		/// <summary>
		/// Initializes a new instance with default values.
		/// </summary>
		public ButtonContent() {
		}

		/// <inheritdoc/>
		public override MessageContentType ContentType => MessageContentType.Button;

		/// <summary>
		/// Gets or sets the text displayed on the button.
		/// </summary>
		public string Text { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the type of the button.
		/// </summary>
		public ButtonType ButtonType { get; set; }

		/// <summary>
		/// Gets or sets the value associated with the button,
		/// such as a URL or callback payload.
		/// </summary>
		public string? Value { get; set; }
	}
}
