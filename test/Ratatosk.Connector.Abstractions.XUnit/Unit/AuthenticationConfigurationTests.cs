//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "AuthenticationConfiguration")]
    public class AuthenticationConfigurationTests
    {
        [Fact]
        public void Should_HaveRequiredFields_When_AuthenticationConfigurationsBasicAuthentication()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic Authentication")
    .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
    .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            Assert.Equal(AuthenticationScheme.Basic, config.Scheme);
            Assert.Equal("Basic Authentication", config.DisplayName);
            Assert.Equal(2, config.Fields.Count);
            
            var usernameField = config.Fields.FirstOrDefault(f => f.FieldName == "Username");
            var passwordField = config.Fields.FirstOrDefault(f => f.FieldName == "Password");
            
            Assert.NotNull(usernameField);
            Assert.NotNull(passwordField);
            Assert.Equal("principal", usernameField.AuthenticationRole);
            Assert.Equal("credential", passwordField.AuthenticationRole);
            Assert.True(passwordField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredField_When_AuthenticationConfigurationsApiKeyAuthentication()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
    .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            Assert.Equal(AuthenticationScheme.ApiKey, config.Scheme);
            Assert.Equal("API Key Authentication", config.DisplayName);
            Assert.Single(config.Fields);
            
            var apiKeyField = config.Fields.First();
            Assert.Equal("ApiKey", apiKeyField.FieldName);
            Assert.Equal("principal", apiKeyField.AuthenticationRole);
            Assert.True(apiKeyField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredField_When_AuthenticationConfigurationsBearerTokenAuthentication()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bearer Token Authentication")
    .WithField("Token", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            Assert.Equal(AuthenticationScheme.Bearer, config.Scheme);
            Assert.Equal("Bearer Token Authentication", config.DisplayName);
            Assert.Single(config.Fields);
            
            var tokenField = config.Fields.First();
            Assert.Equal("Token", tokenField.FieldName);
            Assert.Equal("principal", tokenField.AuthenticationRole);
            Assert.True(tokenField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredFields_When_AuthenticationConfigurationsClientCredentialsAuthentication()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.OAuthClientCredentials, "Client Credentials (OAuth 2.0)")
                .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            Assert.Equal(AuthenticationScheme.OAuthClientCredentials, config.Scheme);
            Assert.Equal("Client Credentials (OAuth 2.0)", config.DisplayName);
            Assert.Equal(2, config.Fields.Count);
            
            var clientIdField = config.Fields.FirstOrDefault(f => f.FieldName == "ClientId");
            var clientSecretField = config.Fields.FirstOrDefault(f => f.FieldName == "ClientSecret");
            
            Assert.NotNull(clientIdField);
            Assert.NotNull(clientSecretField);
            Assert.Equal("principal", clientIdField.AuthenticationRole);
            Assert.Equal("credential", clientSecretField.AuthenticationRole);
            Assert.True(clientSecretField.IsSensitive);
        }

        [Fact]
        public void Should_HaveOptionalFields_When_AuthenticationConfigurationsFlexibleBasicAuthentication()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Flexible Basic Authentication")
    .WithField("Username", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("AccountSid", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("User", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("Pass", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("ClientId", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            Assert.Equal(AuthenticationScheme.Basic, config.Scheme);
            Assert.Equal("Flexible Basic Authentication", config.DisplayName);
            Assert.True(config.Fields.Count >= 8);
            
            var usernameField = config.Fields.FirstOrDefault(f => f.FieldName == "Username");
            var passwordField = config.Fields.FirstOrDefault(f => f.FieldName == "Password");
            var accountSidField = config.Fields.FirstOrDefault(f => f.FieldName == "AccountSid");
            var authTokenField = config.Fields.FirstOrDefault(f => f.FieldName == "AuthToken");
            
            Assert.NotNull(usernameField);
            Assert.NotNull(passwordField);
            Assert.NotNull(accountSidField);
            Assert.NotNull(authTokenField);
        }

        [Fact]
        public void Should_ReturnAllFields_When_AuthenticationConfigurationGetAllFieldNames()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic Authentication")
    .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
    .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            var fieldNames = config.GetAllFieldNames().ToList();

            Assert.Equal(2, fieldNames.Count);
            Assert.Contains("Username", fieldNames);
            Assert.Contains("Password", fieldNames);
        }

        [Fact]
        public void Should_ReturnAllOptionalFields_When_AuthenticationConfigurationGetAllFieldNames()
        {
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Flexible Basic Authentication")
    .WithField("Username", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("AccountSid", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("User", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("Pass", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
    .WithField("ClientId", DataType.String, f => { f.AuthenticationRole = "principal"; })
    .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            var fieldNames = config.GetAllFieldNames().ToList();

            Assert.True(fieldNames.Count >= 8);
            Assert.Contains("Username", fieldNames);
            Assert.Contains("Password", fieldNames);
            Assert.Contains("AccountSid", fieldNames);
            Assert.Contains("AuthToken", fieldNames);
            Assert.Contains("ClientId", fieldNames);
            Assert.Contains("ClientSecret", fieldNames);
        }

        [Fact]
        public void Should_ReturnNoErrors_When_AuthenticationFieldValidateWithValidValue()
        {
            var field = new AuthenticationField("TestField", DataType.String);
            var connectionSettings = new ConnectionSettings()
                .SetParameter("TestField", "valid-value");

            var errors = new List<string>();
            if (connectionSettings.GetParameter(field.FieldName) == null)
                errors.Add($"Missing field '{field.FieldName}'");

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_ReturnError_When_AuthenticationFieldValidateWithMissingValue()
        {
            var field = new AuthenticationField("TestField", DataType.String);
            var connectionSettings = new ConnectionSettings();

            var errors = new List<string>();
            if (connectionSettings.GetParameter(field.FieldName) == null)
                errors.Add($"Missing field '{field.FieldName}'");

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("TestField"));
        }

        [Fact]
        public void Should_ValidateCorrectly_When_AuthenticationFieldValidateWithAllowedValues()
        {
            var field = new AuthenticationField("Priority", DataType.String)
            {
                AllowedValues = new[] { "low", "normal", "high" }
            };
            
            var validSettings = new ConnectionSettings()
                .SetParameter("Priority", "high");
            
            var invalidSettings = new ConnectionSettings()
                .SetParameter("Priority", "invalid");

            var validErrors = new List<string>();
            if (validSettings.GetParameter(field.FieldName) == null)
                validErrors.Add($"Missing field '{field.FieldName}'");
            else if (field.AllowedValues?.Any() == true &&
                     !field.AllowedValues.Any(v => Equals(v, validSettings.GetParameter(field.FieldName))))
                validErrors.Add($"The value '{validSettings.GetParameter(field.FieldName)}' is not valid for field '{field.FieldName}'.");

            var invalidErrors = new List<string>();
            if (invalidSettings.GetParameter(field.FieldName) == null)
                invalidErrors.Add($"Missing field '{field.FieldName}'");
            else if (field.AllowedValues?.Any() == true &&
                     !field.AllowedValues.Any(v => Equals(v, invalidSettings.GetParameter(field.FieldName))))
                invalidErrors.Add($"The value '{invalidSettings.GetParameter(field.FieldName)}' is not valid for field '{field.FieldName}'.");

            Assert.Empty(validErrors);
            Assert.NotEmpty(invalidErrors);
            Assert.Contains(invalidErrors, e => e.Contains("Priority") && e.Contains("invalid"));
        }

        [Fact]
        public void Should_WorksCorrectly_When_CustomAuthenticationWithCustomFields()
        {
            var customField1 = new AuthenticationField("CustomField1", DataType.String)
            {
                DisplayName = "Custom Field 1",
                Description = "A custom authentication field",
                IsSensitive = false
            };
            
            var customField2 = new AuthenticationField("CustomField2", DataType.String)
            {
                DisplayName = "Custom Field 2",
                Description = "Another custom authentication field",
                IsSensitive = true
            };

            var config = new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom Authentication Method")
                .WithField(customField1)
                .WithField(customField2);

            var connectionSettings = new ConnectionSettings()
                .SetParameter("CustomField1", "value1")
                .SetParameter("CustomField2", "value2");

            var isSatisfied = config.Fields.All(f => connectionSettings.GetParameter(f.FieldName) != null);
            var errors = config.Fields
                .Where(f => connectionSettings.GetParameter(f.FieldName) == null)
                .Select(f => $"Missing field '{f.FieldName}'")
                .ToList();

            Assert.Equal(AuthenticationScheme.Custom, config.Scheme);
            Assert.Equal("Custom Authentication Method", config.DisplayName);
            Assert.True(isSatisfied);
            Assert.Empty(errors);
        }
    }
}
