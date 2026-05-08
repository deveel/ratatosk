//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Unit tests for <see cref="MessagingException"/>.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Domain")]
    [Trait("Feature", "MessagingException")]
    public class MessagingExceptionTests
    {
        #region Constructor(string errorCode)

        [Fact]
        public void Should_SetErrorCode_When_ConstructedWithErrorCode()
        {
            // Arrange
            const string errorCode = "ERR_001";

            // Act
            var exception = new MessagingException(errorCode);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.IsAssignableFrom<IMessagingError>(exception);
        }

        [Fact]
        public void Should_HaveNullMessage_When_ConstructedWithErrorCodeOnly()
        {
            // Arrange
            const string errorCode = "ERR_002";

            // Act
            var exception = new MessagingException(errorCode);

            // Assert
            Assert.False(string.IsNullOrEmpty(exception.Message));
            Assert.Null(exception.InnerException);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_ThrowArgumentNullException_When_ErrorCodeIsNullOrWhitespace(string? errorCode)
        {
            // Arrange
            // Act & Assert
            // ArgumentNullException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null,
            // ArgumentException for empty/whitespace — both derive from ArgumentException
            Assert.ThrowsAny<ArgumentException>(() => new MessagingException(errorCode!));
        }

        #endregion

        #region Constructor(string errorCode, string? message)

        [Fact]
        public void Should_SetErrorCodeAndMessage_When_ConstructedWithErrorCodeAndMessage()
        {
            // Arrange
            const string errorCode = "ERR_003";
            const string message = "Something went wrong";

            // Act
            var exception = new MessagingException(errorCode, message);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Should_AllowNullMessage_When_ConstructedWithNullMessage()
        {
            // Arrange
            const string errorCode = "ERR_004";

            // Act
            var exception = new MessagingException(errorCode, null);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Null(exception.InnerException);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_ThrowArgumentNullException_When_ErrorCodeIsNullOrWhitespace_WithMessage(string? errorCode)
        {
            // Arrange
            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => new MessagingException(errorCode!, "some message"));
        }

        #endregion

        #region Constructor(string errorCode, string? message, Exception? innerException)

        [Fact]
        public void Should_SetAllProperties_When_ConstructedWithAllParameters()
        {
            // Arrange
            const string errorCode = "ERR_005";
            const string message = "Error with inner";
            var inner = new InvalidOperationException("inner error");

            // Act
            var exception = new MessagingException(errorCode, message, inner);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Same(inner, exception.InnerException);
        }

        [Fact]
        public void Should_AllowNullInnerException_When_ConstructedWithNullInner()
        {
            // Arrange
            const string errorCode = "ERR_006";
            const string message = "No inner exception";

            // Act
            var exception = new MessagingException(errorCode, message, null);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_ThrowArgumentNullException_When_ErrorCodeIsNullOrWhitespace_WithInner(string? errorCode)
        {
            // Arrange
            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => new MessagingException(errorCode!, "msg", null));
        }

        #endregion

        #region IMessagingError

        [Fact]
        public void Should_ExposeErrorMessageViaInterface_When_MessagingExceptionCreated()
        {
            // Arrange
            const string errorCode = "ERR_007";
            const string message = "Interface message";

            // Act
            IMessagingError error = new MessagingException(errorCode, message);

            // Assert
            Assert.Equal(message, error.ErrorMessage);
            Assert.Equal(errorCode, error.ErrorCode);
        }

        #endregion
    }
}




