using Moq;
using System.Reflection;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Xunit;

namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "TwilioService")]
    public class TwilioServiceMockedTests
    {
        private static TwilioService CreateService(ITwilioApiClient client)
        {
            var ctor = typeof(TwilioService).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(c => c.GetParameters().Length == 1);
            return (TwilioService)ctor.Invoke(new object[] { client });
        }

        [Fact]
        public void Should_Initialize_When_InitializeCalled()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            var service = CreateService(mockClient.Object);

            service.Initialize("AC123", "token");

            mockClient.Verify(x => x.Initialize("AC123", "token"), Times.Once);
        }

        [Fact]
        public async Task Should_FetchAccount_When_FetchAccountAsyncSucceeds()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            mockClient.Setup(x => x.FetchAccountAsync("AC123"))
                .ReturnsAsync((Twilio.Rest.Api.V2010.AccountResource?)null);

            var service = CreateService(mockClient.Object);

            var result = await service.FetchAccountAsync("AC123", TestContext.Current.CancellationToken);

            Assert.Null(result);
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_FetchAccountAsyncThrowsApiException()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            mockClient.Setup(x => x.FetchAccountAsync("AC123"))
                .ThrowsAsync(new ApiException(20001, 400, "Account error", null, null, null));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.FetchAccountAsync("AC123", TestContext.Current.CancellationToken));

            Assert.Equal(MessagingErrorCodes.ConnectionFailed, ex.ErrorCode);
            Assert.Equal(TwilioErrorCodes.ErrorDomain, ex.ErrorDomain);
        }

        [Fact]
        public async Task Should_CreateMessage_When_CreateMessageAsyncSucceeds()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            var options = new CreateMessageOptions("+1234567890")
            {
                Body = "Test message"
            };
            var mockResult = TwilioMockFactory.CreateMockMessageResource("SM123", MessageResource.StatusEnum.Queued);
            mockClient.Setup(x => x.CreateMessageAsync(options))
                .ReturnsAsync(mockResult);

            var service = CreateService(mockClient.Object);

            var result = await service.CreateMessageAsync(options, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal("SM123", result.Sid);
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_CreateMessageAsyncThrowsApiException()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            var options = new CreateMessageOptions("+1234567890");
            mockClient.Setup(x => x.CreateMessageAsync(options))
                .ThrowsAsync(new ApiException(21211, 400, "Invalid number", null, null, null));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.CreateMessageAsync(options, TestContext.Current.CancellationToken));

            Assert.Equal(MessagingErrorCodes.InvalidRecipient, ex.ErrorCode);
            Assert.Equal(TwilioErrorCodes.ErrorDomain, ex.ErrorDomain);
        }

        [Fact]
        public async Task Should_FetchMessage_When_FetchMessageAsyncSucceeds()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            var mockResult = TwilioMockFactory.CreateMockMessageResource("SM123", MessageResource.StatusEnum.Sent);
            mockClient.Setup(x => x.FetchMessageAsync("SM123"))
                .ReturnsAsync(mockResult);

            var service = CreateService(mockClient.Object);

            var result = await service.FetchMessageAsync("SM123", TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal("SM123", result.Sid);
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_FetchMessageAsyncThrowsApiException()
        {
            var mockClient = new Mock<ITwilioApiClient>();
            mockClient.Setup(x => x.FetchMessageAsync("SM123"))
                .ThrowsAsync(new ApiException(20429, 429, "Rate limited", null, null, null));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.FetchMessageAsync("SM123", TestContext.Current.CancellationToken));

            Assert.Equal(TwilioErrorCodes.StatusQueryFailed, ex.ErrorCode);
            Assert.Equal(TwilioErrorCodes.ErrorDomain, ex.ErrorDomain);
        }
    }
}
