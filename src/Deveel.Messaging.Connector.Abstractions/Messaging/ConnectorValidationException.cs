using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

public class ConnectorValidationException : ConnectorException
{
    public ConnectorValidationException(string errorCode, IReadOnlyList<ValidationResult> validationResults) : base(errorCode)
    {
        ValidationResults = validationResults;
    }

    public ConnectorValidationException(string errorCode, string? message, IReadOnlyList<ValidationResult> validationResults) : base(errorCode, message)
    {
        ValidationResults = validationResults;
    }

    /// <summary>
    /// Gets the collection of validation results.
    /// </summary>
    public IReadOnlyList<ValidationResult> ValidationResults { get; }
}
