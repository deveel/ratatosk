namespace Deveel.Messaging {
	/// <summary>
	/// Defines the types of buttons that can be used in interactive content.
	/// </summary>
	public enum ButtonType {
		/// <summary>Opens a URL when clicked.</summary>
		Url = 0,
		/// <summary>Sends a callback payload to the bot.</summary>
		Postback = 1,
		/// <summary>Prompts the user to make a phone call.</summary>
		PhoneNumber = 2
	}
}
