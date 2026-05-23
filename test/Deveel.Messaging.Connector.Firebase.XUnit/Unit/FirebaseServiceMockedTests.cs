using FirebaseAdmin.Messaging;
using Moq;
using System.Reflection;
using Xunit;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebaseService")]
    public class FirebaseServiceMockedTests
    {
        private static FirebaseService CreateService(IFirebaseMessagingClient client)
        {
            var ctor = typeof(FirebaseService).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(c => c.GetParameters().Length == 1);
            return (FirebaseService)ctor.Invoke(new object[] { client });
        }

        private static FirebaseAdmin.Messaging.Message CreateMessage(string token = "test-token")
            => new FirebaseAdmin.Messaging.Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = "Test",
                    Body = "Test body"
                }
            };

        [Fact]
        public async Task Should_SendMessage_When_SendAsyncSucceeds()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var message = CreateMessage();
            mockClient.Setup(x => x.SendAsync(message, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync("msg-123");

            var service = CreateService(mockClient.Object);

            var result = await service.SendAsync(message, false, TestContext.Current.CancellationToken);

            Assert.Equal("msg-123", result);
            mockClient.Verify(x => x.SendAsync(message, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_SendAsyncThrowsException()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var message = CreateMessage();
            mockClient.Setup(x => x.SendAsync(message, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("network error"));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.SendAsync(message, false, TestContext.Current.CancellationToken));

            Assert.Equal(MessagingErrorCodes.SendMessageFailed, ex.ErrorCode);
            Assert.Equal(FirebaseErrorCodes.ErrorDomain, ex.ErrorDomain);
        }

        [Fact]
        public async Task Should_ThrowInvalidOperation_When_SendAsyncNotInitialized()
        {
            var service = new FirebaseService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendAsync(CreateMessage(), false, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_SendEachAsyncThrowsException()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var messages = new[] { CreateMessage() };
            mockClient.Setup(x => x.SendEachAsync(messages, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("error"));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.SendEachAsync(messages, false, TestContext.Current.CancellationToken));

            Assert.Equal(MessagingErrorCodes.SendMessageFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task Should_ThrowConnectorException_When_SendMulticastAsyncThrowsException()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var message = new MulticastMessage { Tokens = new[] { "token1" } };
            mockClient.Setup(x => x.SendMulticastAsync(message, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("error"));

            var service = CreateService(mockClient.Object);

            var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
                service.SendMulticastAsync(message, false, TestContext.Current.CancellationToken));

            Assert.Equal(MessagingErrorCodes.SendMessageFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnTrue_When_TestConnectionAsyncSendsSuccessfully()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            mockClient.Setup(x => x.SendAsync(It.IsAny<FirebaseAdmin.Messaging.Message>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync("test-msg-id");

            var service = CreateService(mockClient.Object);

            var result = await service.TestConnectionAsync(TestContext.Current.CancellationToken);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_ReturnFalse_When_TestConnectionAsyncNotInitialized()
        {
            var service = new FirebaseService();

            var result = await service.TestConnectionAsync(TestContext.Current.CancellationToken);

            Assert.False(result);
        }

        [Fact]
        public void Should_BeInitialized_When_MockClientProvided()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var service = CreateService(mockClient.Object);

            Assert.True(service.IsInitialized);
            Assert.Null(service.App);
        }

        [Fact]
        public async Task Should_ThrowArgumentNullException_When_SendAsyncNullMessage()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var service = CreateService(mockClient.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.SendAsync(null!, false, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Should_ThrowArgumentNullException_When_SendEachAsyncNullMessages()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var service = CreateService(mockClient.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.SendEachAsync(null!, false, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Should_ThrowArgumentNullException_When_SendMulticastAsyncNullMessage()
        {
            var mockClient = new Mock<IFirebaseMessagingClient>();
            var service = CreateService(mockClient.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.SendMulticastAsync(null!, false, TestContext.Current.CancellationToken));
        }
    }
}
