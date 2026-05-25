using Microsoft.Extensions.Logging;

namespace Ratatosk;

/// <summary>
/// Defines base event identifiers used by messaging connectors.
/// </summary>
public static class LoggerEventId
{
    /// <summary>
    /// The starting event identifier for connector logging events.
    /// </summary>
    public const int BaseId = 1001;
}
