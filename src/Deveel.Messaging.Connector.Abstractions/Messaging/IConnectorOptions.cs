namespace Deveel.Messaging
{
    public interface IConnectorOptions
    {
        ConnectionSettings ToConnectionSettings();
    }
}
