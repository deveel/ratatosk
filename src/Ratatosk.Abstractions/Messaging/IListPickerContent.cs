namespace Ratatosk {
	/// <summary>
	/// Represents a list picker content that allows the user
	/// to select from a vertical list of items.
	/// </summary>
	public interface IListPickerContent : IInteractiveContent {
		/// <summary>
		/// Gets the title of the list picker.
		/// </summary>
		string? Title { get; }

		/// <summary>
		/// Gets an optional subtitle displayed above the list.
		/// </summary>
		string? Subtitle { get; }

		/// <summary>
		/// Gets a read-only list of selectable items.
		/// </summary>
		IReadOnlyList<IListPickerItem> Items { get; }

		/// <summary>
		/// Gets the display style of the list picker.
		/// </summary>
		ListPickerStyle Style { get; }
	}
}
