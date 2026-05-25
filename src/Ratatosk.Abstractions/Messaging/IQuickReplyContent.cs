namespace Ratatosk {
	/// <summary>
	/// Represents a quick reply option that can be presented
	/// to the user as a selectable response.
	/// </summary>
	public interface IQuickReplyContent : IInteractiveContent {
		/// <summary>
		/// Gets the title of the quick reply option.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets an optional payload sent back when the quick
		/// reply is selected.
		/// </summary>
		string? Payload { get; }

		/// <summary>
		/// Gets an optional URL of an image to display alongside
		/// the quick reply option.
		/// </summary>
		string? ImageUrl { get; }
	}
}
