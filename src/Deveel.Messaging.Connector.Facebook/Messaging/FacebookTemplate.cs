namespace Deveel.Messaging {
	/// <summary>
	/// Represents a structured template payload used in Facebook
	/// Messenger messages (e.g., button, generic, list templates).
	/// </summary>
	public class FacebookTemplate {
		/// <summary>
		/// Gets or sets the type of the template (e.g., "button", "generic", "list").
		/// </summary>
		public string TemplateType { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the payload dictionary containing template-specific data.
		/// </summary>
		public Dictionary<string, object> Payload { get; set; } = new();
	}
}
