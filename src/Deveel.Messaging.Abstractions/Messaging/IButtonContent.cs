namespace Deveel.Messaging {
	/// <summary>
	/// Represents a content that provides a clickable button
	/// in a message.
	/// </summary>
	public interface IButtonContent : IInteractiveContent {
		/// <summary>
		/// Gets the text displayed on the button.
		/// </summary>
		string Text { get; }

		/// <summary>
		/// Gets the type of the button that defines its behavior.
		/// </summary>
		ButtonType ButtonType { get; }

		/// <summary>
		/// Gets the value associated with the button,
		/// such as a URL or callback payload.
		/// </summary>
		string? Value { get; }
	}
}
