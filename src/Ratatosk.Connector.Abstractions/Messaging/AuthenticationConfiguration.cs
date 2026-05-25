//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Describes the set of fields required or accepted by a particular <see cref="AuthenticationScheme"/>
    /// when authenticating against a messaging channel.
    /// </summary>
    /// <remarks>
    /// This class is purely descriptive — it defines <em>what</em> fields a provider needs.
    /// Validation of actual values against a <see cref="ConnectionSettings"/> instance is
    /// performed by the <see cref="IAuthenticationProvider"/>, not by this class.
    /// </remarks>
    public class AuthenticationConfiguration
    {
        /// <summary>
        /// Initializes a new instance with the given <paramref name="scheme"/>.
        /// </summary>
        /// <param name="scheme">The authentication scheme this configuration describes.</param>
        /// <param name="displayName">An optional human-readable name; defaults to <c>scheme.Name</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="scheme"/> is <c>null</c>.</exception>
        public AuthenticationConfiguration(AuthenticationScheme scheme, string? displayName = null)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            DisplayName = displayName ?? scheme.Name;
            Fields = new List<AuthenticationField>();
        }

        /// <summary>
        /// Gets the authentication scheme this configuration is for.
        /// </summary>
        public AuthenticationScheme Scheme { get; }

        /// <summary>
        /// Gets the human-readable display name of this authentication method.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the list of fields that this authentication method requires or accepts.
        /// </summary>
        /// <remarks>
        /// Fields are matched to providers by their <see cref="AuthenticationField.AuthenticationRole"/>
        /// value. A provider for a given scheme will look for fields with well-known role names
        /// (e.g. <c>"principal"</c>, <c>"credential"</c>).
        /// </remarks>
        public IList<AuthenticationField> Fields { get; }

        /// <summary>
        /// Adds a pre-configured <see cref="AuthenticationField"/> to this configuration.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>This instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <c>null</c>.</exception>
        public AuthenticationConfiguration WithField(AuthenticationField field)
        {
            ArgumentNullException.ThrowIfNull(field, nameof(field));
            Fields.Add(field);
            return this;
        }

        /// <summary>
        /// Creates an <see cref="AuthenticationField"/> with the given <paramref name="fieldName"/>
        /// and <paramref name="dataType"/>, optionally configures it, and adds it to this configuration.
        /// </summary>
        /// <param name="fieldName">The connection-settings key this field maps to.</param>
        /// <param name="dataType">The expected data type of the field value.</param>
        /// <param name="configure">An optional action to set additional properties (e.g. role, sensitivity).</param>
        /// <returns>This instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public AuthenticationConfiguration WithField(string fieldName, DataType dataType, Action<AuthenticationField>? configure = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
            var field = new AuthenticationField(fieldName, dataType);
            configure?.Invoke(field);
            return WithField(field);
        }

        /// <summary>
        /// Returns all fields whose <see cref="AuthenticationField.AuthenticationRole"/>
        /// matches <paramref name="role"/> (case-insensitive).
        /// </summary>
        /// <param name="role">The role name to filter by.</param>
        /// <returns>Zero or more matching fields.</returns>
        public IEnumerable<AuthenticationField> GetFieldsByRole(string role)
        {
            return Fields.Where(f => string.Equals(f.AuthenticationRole, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the names of every field defined in this configuration.
        /// </summary>
        /// <returns>All field names.</returns>
        public IEnumerable<string> GetAllFieldNames()
        {
            return Fields.Select(f => f.FieldName);
        }

        /// <summary>
        /// Determines whether this configuration is satisfied by the given connection settings,
        /// using role-aware matching: for configs with <c>principal</c>+<c>credential</c> roles,
        /// at least one of each must be present; for configs with only <c>principal</c>,
        /// at least one must be present; all other fields must be present.
        /// </summary>
        /// <param name="connectionSettings">The settings to check.</param>
        /// <returns><c>true</c> if the configuration is satisfied.</returns>
        public bool IsSatisfiedBy(ConnectionSettings connectionSettings)
        {
            var principalFields = Fields.Where(f => string.Equals(f.AuthenticationRole, "principal", StringComparison.OrdinalIgnoreCase)).ToList();
            var credentialFields = Fields.Where(f => string.Equals(f.AuthenticationRole, "credential", StringComparison.OrdinalIgnoreCase)).ToList();
            var otherFields = Fields.Where(f => !string.Equals(f.AuthenticationRole, "principal", StringComparison.OrdinalIgnoreCase) && !string.Equals(f.AuthenticationRole, "credential", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!otherFields.All(f => connectionSettings.GetParameter(f.FieldName) != null))
                return false;

            if (principalFields.Any() && credentialFields.Any())
                return principalFields.Any(f => connectionSettings.GetParameter(f.FieldName) != null) &&
                       credentialFields.Any(f => connectionSettings.GetParameter(f.FieldName) != null);

            if (principalFields.Any())
                return principalFields.Any(f => connectionSettings.GetParameter(f.FieldName) != null);

            if (credentialFields.Any())
                return credentialFields.Any(f => connectionSettings.GetParameter(f.FieldName) != null);

            return Fields.Any(f => connectionSettings.GetParameter(f.FieldName) != null);
        }
    }
}
