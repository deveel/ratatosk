//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Tests for authentication configurations and their interaction with connection settings.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "AuthenticationConfiguration")]
    public class AuthenticationConfigurationTests
    {
        [Fact]
        public void Should_HaveRequiredFields_When_AuthenticationConfigurationsBasicAuthentication()
        {
            // Arrange
            // Act
            var config = AuthenticationConfigurations.BasicAuthentication();

            // Assert
            Assert.Equal(AuthenticationType.Basic, config.AuthenticationType);
            Assert.Equal("Basic Authentication", config.DisplayName);
            Assert.Equal(2, config.RequiredFields.Count);
            
            var usernameField = config.RequiredFields.FirstOrDefault(f => f.FieldName == "Username");
            var passwordField = config.RequiredFields.FirstOrDefault(f => f.FieldName == "Password");
            
            Assert.NotNull(usernameField);
            Assert.NotNull(passwordField);
            Assert.Equal("Username", usernameField.AuthenticationRole);
            Assert.Equal("Password", passwordField.AuthenticationRole);
            Assert.True(passwordField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredField_When_AuthenticationConfigurationsApiKeyAuthentication()
        {
            // Arrange
            // Act
            var config = AuthenticationConfigurations.ApiKeyAuthentication();

            // Assert
            Assert.Equal(AuthenticationType.ApiKey, config.AuthenticationType);
            Assert.Equal("API Key Authentication", config.DisplayName);
            Assert.Single(config.RequiredFields);
            
            var apiKeyField = config.RequiredFields.First();
            Assert.Equal("ApiKey", apiKeyField.FieldName);
            Assert.Equal("ApiKey", apiKeyField.AuthenticationRole);
            Assert.True(apiKeyField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredField_When_AuthenticationConfigurationsTokenAuthentication()
        {
            // Arrange
            // Act
            var config = AuthenticationConfigurations.TokenAuthentication();

            // Assert
            Assert.Equal(AuthenticationType.Token, config.AuthenticationType);
            Assert.Equal("Token Authentication", config.DisplayName);
            Assert.Single(config.RequiredFields);
            
            var tokenField = config.RequiredFields.First();
            Assert.Equal("Token", tokenField.FieldName);
            Assert.Equal("Token", tokenField.AuthenticationRole);
            Assert.True(tokenField.IsSensitive);
        }

        [Fact]
        public void Should_HaveRequiredFields_When_AuthenticationConfigurationsClientCredentialsAuthentication()
        {
            // Arrange
            // Act
            var config = AuthenticationConfigurations.ClientCredentialsAuthentication();

            // Assert
            Assert.Equal(AuthenticationType.ClientCredentials, config.AuthenticationType);
            Assert.Equal("Client Credentials Authentication", config.DisplayName);
            Assert.Equal(2, config.RequiredFields.Count);
            
            var clientIdField = config.RequiredFields.FirstOrDefault(f => f.FieldName == "ClientId");
            var clientSecretField = config.RequiredFields.FirstOrDefault(f => f.FieldName == "ClientSecret");
            
            Assert.NotNull(clientIdField);
            Assert.NotNull(clientSecretField);
            Assert.Equal("ClientId", clientIdField.AuthenticationRole);
            Assert.Equal("ClientSecret", clientSecretField.AuthenticationRole);
            Assert.True(clientSecretField.IsSensitive);
        }

        [Fact]
        public void Should_HaveOptionalFields_When_AuthenticationConfigurationsFlexibleBasicAuthentication()
        {
            // Arrange
            // Act
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();

            // Assert
            Assert.Equal(AuthenticationType.Basic, config.AuthenticationType);
            Assert.Equal("Flexible Basic Authentication", config.DisplayName);
            Assert.Empty(config.RequiredFields); // Flexible configs use optional fields
            Assert.True(config.OptionalFields.Count >= 8); // Multiple optional field combinations
            
            // Check for expected field pairs
            var usernameField = config.OptionalFields.FirstOrDefault(f => f.FieldName == "Username");
            var passwordField = config.OptionalFields.FirstOrDefault(f => f.FieldName == "Password");
            var accountSidField = config.OptionalFields.FirstOrDefault(f => f.FieldName == "AccountSid");
            var authTokenField = config.OptionalFields.FirstOrDefault(f => f.FieldName == "AuthToken");
            
            Assert.NotNull(usernameField);
            Assert.NotNull(passwordField);
            Assert.NotNull(accountSidField);
            Assert.NotNull(authTokenField);
        }

        [Fact]
        public void Should_ReturnNoErrors_When_AuthenticationConfigurationValidateWithValidBasicAuth()
        {
            // Arrange
            var config = AuthenticationConfigurations.BasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("Password", "testpass");

            // Act
            var errors = config.Validate(connectionSettings);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_ReturnError_When_AuthenticationConfigurationValidateWithMissingPassword()
        {
            // Arrange
            var config = AuthenticationConfigurations.BasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser");
                // Missing Password

            // Act
            var errors = config.Validate(connectionSettings);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Password"));
        }

        [Fact]
        public void Should_ReturnTrue_When_AuthenticationConfigurationIsSatisfiedByWithValidCredentials()
        {
            // Arrange
            var config = AuthenticationConfigurations.BasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("Password", "testpass");

            // Act
            var isSatisfied = config.IsSatisfiedBy(connectionSettings);

            // Assert
            Assert.True(isSatisfied);
        }

        [Fact]
        public void Should_ReturnFalse_When_AuthenticationConfigurationIsSatisfiedByWithMissingCredentials()
        {
            // Arrange
            var config = AuthenticationConfigurations.BasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser");
                // Missing Password

            // Act
            var isSatisfied = config.IsSatisfiedBy(connectionSettings);

            // Assert
            Assert.False(isSatisfied);
        }

        [Fact]
        public void Should_ReturnNoErrors_When_FlexibleAuthenticationConfigurationValidateWithValidPair()
        {
            // Arrange
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("AccountSid", "AC123456")
                .SetParameter("AuthToken", "token123");

            // Act
            var errors = config.Validate(connectionSettings);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_ReturnError_When_FlexibleAuthenticationConfigurationValidateWithMismatchedPair()
        {
            // Arrange
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("AuthToken", "token123"); // Mismatched pair

            // Act
            var errors = config.Validate(connectionSettings);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("parameter pairs"));
        }

        [Fact]
        public void Should_ReturnTrue_When_FlexibleAuthenticationConfigurationIsSatisfiedByWithValidPair()
        {
            // Arrange
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ClientId", "client123")
                .SetParameter("ClientSecret", "secret456");

            // Act
            var isSatisfied = config.IsSatisfiedBy(connectionSettings);

            // Assert
            Assert.True(isSatisfied);
        }

        [Fact]
        public void Should_ReturnFalse_When_FlexibleAuthenticationConfigurationIsSatisfiedByWithNoPairs()
        {
            // Arrange
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("SomeOtherField", "value");

            // Act
            var isSatisfied = config.IsSatisfiedBy(connectionSettings);

            // Assert
            Assert.False(isSatisfied);
        }

        [Fact]
        public void Should_ReturnAllFields_When_AuthenticationConfigurationGetAllFieldNames()
        {
            // Arrange
            var config = AuthenticationConfigurations.BasicAuthentication();

            // Act
            var fieldNames = config.GetAllFieldNames().ToList();

            // Assert
            Assert.Equal(2, fieldNames.Count);
            Assert.Contains("Username", fieldNames);
            Assert.Contains("Password", fieldNames);
        }

        [Fact]
        public void Should_ReturnAllOptionalFields_When_FlexibleAuthenticationConfigurationGetAllFieldNames()
        {
            // Arrange
            var config = AuthenticationConfigurations.FlexibleBasicAuthentication();

            // Act
            var fieldNames = config.GetAllFieldNames().ToList();

            // Assert
            Assert.True(fieldNames.Count >= 8); // Should have multiple optional fields
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
            // Arrange
            var field = new AuthenticationField("TestField", DataType.String);
            var connectionSettings = new ConnectionSettings()
                .SetParameter("TestField", "valid-value");

            // Act
            var errors = field.Validate(connectionSettings);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_ReturnError_When_AuthenticationFieldValidateWithMissingValue()
        {
            // Arrange
            var field = new AuthenticationField("TestField", DataType.String);
            var connectionSettings = new ConnectionSettings();

            // Act
            var errors = field.Validate(connectionSettings);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("TestField"));
        }

        [Fact]
        public void Should_ValidateCorrectly_When_AuthenticationFieldValidateWithAllowedValues()
        {
            // Arrange
            var field = new AuthenticationField("Priority", DataType.String)
            {
                AllowedValues = new[] { "low", "normal", "high" }
            };
            
            var validSettings = new ConnectionSettings()
                .SetParameter("Priority", "high");
            
            var invalidSettings = new ConnectionSettings()
                .SetParameter("Priority", "invalid");

            // Act
            var validErrors = field.Validate(validSettings);
            var invalidErrors = field.Validate(invalidSettings);

            // Assert
            Assert.Empty(validErrors);
            Assert.NotEmpty(invalidErrors);
            Assert.Contains(invalidErrors, e => e.Contains("Priority") && e.Contains("invalid"));
        }

        [Fact]
        public void Should_WorksCorrectly_When_CustomAuthenticationWithCustomFields()
        {
            // Arrange
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

            var config = AuthenticationConfigurations.CustomAuthentication(
                "Custom Authentication Method",
                new[] { customField1 },
                new[] { customField2 }
            );

            // Act
            var connectionSettings = new ConnectionSettings()
                .SetParameter("CustomField1", "value1")
                .SetParameter("CustomField2", "value2");

            var isSatisfied = config.IsSatisfiedBy(connectionSettings);
            var errors = config.Validate(connectionSettings);

            // Assert
            Assert.Equal(AuthenticationType.Custom, config.AuthenticationType);
            Assert.Equal("Custom Authentication Method", config.DisplayName);
            Assert.True(isSatisfied);
            Assert.Empty(errors);
        }
    }
}