//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Unit tests for <see cref="MessageValidationException"/>.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Domain")]
    [Trait("Feature", "MessageValidationException")]
    public class MessageValidationExceptionTests
    {
        private static IReadOnlyList<ValidationResult> BuildValidationResults(int count = 1)
        {
            return Enumerable.Range(1, count)
                .Select(i => new ValidationResult($"Validation error {i}", new[] { $"Field{i}" }))
                .ToList();
        }

        #region Constructor(string errorCode, string? message, IReadOnlyList<ValidationResult> validationResults)

        [Fact]
        public void Should_SetAllProperties_When_ConstructedWithCodeMessageAndResults()
        {
            // Arrange
            const string errorCode = "VALIDATION_001";
            const string message = "Message validation failed";
            var validationResults = BuildValidationResults(2);

            // Act
            var exception = new MessageValidationException(errorCode, message, validationResults);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Same(validationResults, exception.ValidationResults);
            Assert.Equal(2, exception.ValidationResults.Count);
        }

        [Fact]
        public void Should_AllowNullMessage_When_ConstructedWithNullMessage()
        {
            // Arrange
            const string errorCode = "VALIDATION_002";
            var validationResults = BuildValidationResults();

            // Act
            var exception = new MessageValidationException(errorCode, null, validationResults);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.NotNull(exception.ValidationResults);
        }

        [Fact]
        public void Should_ImplementIMessageValidationError_When_Created()
        {
            // Arrange
            const string errorCode = "VALIDATION_003";
            var results = BuildValidationResults(3);

            // Act
            IMessageValidationError error = new MessageValidationException(errorCode, "error", results);

            // Assert
            Assert.Equal(errorCode, error.ErrorCode);
            Assert.Equal(3, error.ValidationResults.Count);
        }

        #endregion

        #region Constructor(string errorCode, IReadOnlyList<ValidationResult> validationResults)

        [Fact]
        public void Should_SetProperties_When_ConstructedWithCodeAndResults()
        {
            // Arrange
            const string errorCode = "VALIDATION_004";
            var validationResults = BuildValidationResults(1);

            // Act
            var exception = new MessageValidationException(errorCode, validationResults);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Same(validationResults, exception.ValidationResults);
            Assert.Single(exception.ValidationResults);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Should_ThrowArgumentNullException_When_ErrorCodeIsNullOrWhitespace(string? errorCode)
        {
            // Arrange
            var results = BuildValidationResults();

            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => new MessageValidationException(errorCode!, results));
        }

        #endregion
    }
}


