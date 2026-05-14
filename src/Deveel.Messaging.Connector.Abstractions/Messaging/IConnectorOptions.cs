namespace Deveel.Messaging
{
    /// <summary>
    /// Defines the options for configuring a channel connector.
    /// </summary>
    public interface IConnectorOptions
    {
        /// <summary>
        /// Converts the options into a <see cref="ConnectionSettings"/> instance
        /// that can be used to configure the connector.
        /// </summary>
        /// <returns>
        /// A <see cref="ConnectionSettings"/> containing the parameters
        /// derived from this options instance.
        /// </returns>
        ConnectionSettings ToConnectionSettings();
    }
}
