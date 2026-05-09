namespace Deveel.Messaging;

/// <summary>
/// Represents an error that occurs during connector operations.
/// </summary>
public class ConnectorException : MessagingException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorException"/> class.
    /// </summary>
    /// <param name="errorCode">The connector-specific error code.</param>
    public ConnectorException(string errorCode) : base(errorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorException"/> class.
    /// </summary>
    /// <param name="errorCode">The connector-specific error code.</param>
    /// <param name="message">The error message that describes the current exception.</param>
    public ConnectorException(string errorCode, string? message) : base(errorCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorException"/> class.
    /// </summary>
    /// <param name="errorCode">The connector-specific error code.</param>
    /// <param name="message">The error message that describes the current exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConnectorException(string errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
    {
    }
}
