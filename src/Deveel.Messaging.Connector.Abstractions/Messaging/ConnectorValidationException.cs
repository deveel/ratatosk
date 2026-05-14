using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Represents a connector error caused by one or more validation failures.
/// </summary>
public class ConnectorValidationException : ConnectorException, IValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorValidationException"/> class.
    /// </summary>
    /// <param name="errorCode">The connector-specific error code.</param>
    /// <param name="errorDomain">The domain or category of the error.</param>
    /// <param name="validationResults">The validation errors associated with the exception.</param>
    public ConnectorValidationException(string errorCode, string errorDomain, IReadOnlyList<ValidationResult> validationResults) 
        : base(errorCode, errorDomain)
    {
        ValidationResults = validationResults;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorValidationException"/> class.
    /// </summary>
    /// <param name="errorCode">The connector-specific error code.</param>
    /// <param name="errorDomain">The domain or category of the error.</param>
    /// <param name="message">The error message that describes the current exception.</param>
    /// <param name="validationResults">The validation errors associated with the exception.</param>
    public ConnectorValidationException(string errorCode, string errorDomain, string? message, IReadOnlyList<ValidationResult> validationResults) 
        : base(errorCode, errorDomain, message)
    {
        ValidationResults = validationResults;
    }

    /// <summary>
    /// Gets the collection of validation results.
    /// </summary>
    public IReadOnlyList<ValidationResult> ValidationResults { get; }
}
