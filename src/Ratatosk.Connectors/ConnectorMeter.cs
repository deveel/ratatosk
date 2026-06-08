namespace Ratatosk;

/// <summary>
/// An helper class for creating meter names.
/// </summary>
public static class ConnectorMeter
{
    /// <summary>
    /// The base name for all connector meters.
    /// </summary>
    public const string Name = "Ratatosk.Connector";
    
    internal static string MakeConnectorName(string connectorType) => $"{Name}.{connectorType}";
}