//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Unit tests for <see cref="MessageValidationError"/> struct.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Domain")]
    [Trait("Feature", "MessageValidationError")]
    public class MessageValidationErrorTests
    {
        private static IReadOnlyList<ValidationResult> SampleResults(int count = 1) =>
            Enumerable.Range(1, count)
                .Select(i => new ValidationResult($"Error {i}", new[] { $"Field{i}" }))
                .ToList();

        #region Constructor(string, string?, IReadOnlyList<ValidationResult>)

        [Fact]
        public void Should_SetAllProperties_When_ConstructedWithCodeMessageAndResults()
        {
            // Arrange
            const string errorCode = "ERR_VALIDATION";
            const string errorMessage = "Validation failed badly";
            var results = SampleResults(2);

            // Act
            var error = new MessageValidationError(errorCode, errorMessage, results);

            // Assert
            Assert.Equal(errorCode, error.ErrorCode);
            Assert.Equal(errorMessage, error.ErrorMessage);
            Assert.Equal(2, error.ValidationResults.Count);
        }

        [Fact]
        public void Should_AllowNullErrorMessage_When_ConstructedWithNullMessage()
        {
            // Arrange
            const string errorCode = "ERR_VALIDATION_NULL";
            var results = SampleResults();

            // Act
            var error = new MessageValidationError(errorCode, null, results);

            // Assert
            Assert.Equal(errorCode, error.ErrorCode);
            Assert.Null(error.ErrorMessage);
            Assert.NotNull(error.ValidationResults);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_ThrowArgumentException_When_ErrorCodeIsNullOrWhitespace(string? errorCode)
        {
            // Arrange
            var results = SampleResults();

            // Act & Assert
            // ArgumentNullException.ThrowIfNullOrWhiteSpace → ArgumentNullException for null,
            // ArgumentException for empty/whitespace (both extend ArgumentException)
            Assert.ThrowsAny<ArgumentException>(() =>
                new MessageValidationError(errorCode!, "message", results));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ValidationResultsIsNull()
        {
            // Arrange
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new MessageValidationError("ERR", "message", null!));
        }

        #endregion

        #region Constructor(string, IReadOnlyList<ValidationResult>)

        [Fact]
        public void Should_SetNullErrorMessage_When_ConstructedWithCodeAndResults()
        {
            // Arrange
            const string errorCode = "ERR_SHORT";
            var results = SampleResults(3);

            // Act
            var error = new MessageValidationError(errorCode, results);

            // Assert
            Assert.Equal(errorCode, error.ErrorCode);
            Assert.Null(error.ErrorMessage);
            Assert.Equal(3, error.ValidationResults.Count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_ThrowArgumentException_When_ErrorCodeIsNullOrWhitespace_TwoParam(string? errorCode)
        {
            // Arrange
            var results = SampleResults();

            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                new MessageValidationError(errorCode!, results));
        }

        [Fact]
        public void Should_ImplementIMessageValidationError_When_Created()
        {
            // Arrange
            const string errorCode = "ERR_INTERFACE";
            var results = SampleResults(1);

            // Act
            IMessageValidationError iface = new MessageValidationError(errorCode, "err msg", results);

            // Assert
            Assert.Equal(errorCode, iface.ErrorCode);
            Assert.Equal("err msg", iface.ErrorMessage);
            Assert.Single(iface.ValidationResults);
        }

        #endregion
    }
}



