//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Describes a single field that an <see cref="AuthenticationConfiguration"/> expects
    /// to find in <see cref="ConnectionSettings"/>.
    /// </summary>
    /// <remarks>
    /// This is a purely descriptive class. Validation of actual values is handled by
    /// <see cref="IAuthenticationProvider"/> implementations.
    /// </remarks>
    public sealed class AuthenticationField
    {
        /// <summary>
        /// Initializes a new instance with the given <paramref name="fieldName"/> and <paramref name="dataType"/>.
        /// </summary>
        /// <param name="fieldName">The connection-settings parameter key.</param>
        /// <param name="dataType">The expected data type of the value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public AuthenticationField(string fieldName, DataType dataType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
            FieldName = fieldName;
            DataType = dataType;
            IsSensitive = false;
            AllowedValues = null;
        }

        /// <summary>
        /// Gets the connection-settings parameter key that this field maps to.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the expected data type of the field value.
        /// </summary>
        public DataType DataType { get; }

        /// <summary>
        /// Gets or sets whether this field contains sensitive information (e.g. passwords, tokens).
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// Gets or sets an optional human-readable name for this field.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets an optional description of this field.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets an optional set of values that are allowed for this field.
        /// </summary>
        /// <remarks>
        /// When set and non-empty, providers may reject values not in this list.
        /// </remarks>
        public IList<object>? AllowedValues { get; set; }

        /// <summary>
        /// Gets or sets the semantic role this field plays (e.g. <c>"principal"</c>, <c>"credential"</c>).
        /// </summary>
        /// <remarks>
        /// Providers use the role to locate the fields they need, so roles must match
        /// what the provider expects (e.g. <c>"principal"</c> for a username or key,
        /// <c>"credential"</c> for a password or secret).
        /// </remarks>
        public string? AuthenticationRole { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var displayText = DisplayName ?? FieldName;
            var role = AuthenticationRole != null ? $" ({AuthenticationRole})" : "";
            return $"{displayText}: {DataType}{role}";
        }
    }
}
