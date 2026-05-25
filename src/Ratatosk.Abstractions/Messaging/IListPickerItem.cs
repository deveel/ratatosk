namespace Ratatosk {
	/// <summary>
	/// Represents a single item in a list picker selection.
	/// </summary>
	public interface IListPickerItem {
		/// <summary>
		/// Gets the title of the list item.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets an optional description of the item.
		/// </summary>
		string? Description { get; }

		/// <summary>
		/// Gets an optional URL of an image for the item.
		/// </summary>
		string? ImageUrl { get; }

		/// <summary>
		/// Gets an optional payload returned when the item is selected.
		/// </summary>
		string? Payload { get; }
	}
}
