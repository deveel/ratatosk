namespace Deveel.Messaging;

public class ConnectorException : MessagingException
{
    public ConnectorException(string errorCode) : base(errorCode)
    {
    }

    public ConnectorException(string errorCode, string? message) : base(errorCode, message)
    {
    }

    public ConnectorException(string errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
    {
    }
}
