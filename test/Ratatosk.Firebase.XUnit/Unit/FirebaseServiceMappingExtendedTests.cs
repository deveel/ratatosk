using FirebaseAdmin.Messaging;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FirebaseService")]
public class FirebaseServiceMappingExtendedTests
{
    [Theory]
    [InlineData(MessagingErrorCode.InvalidArgument, FirebaseErrorCodes.InvalidArgument)]
    [InlineData(MessagingErrorCode.Unregistered, FirebaseErrorCodes.UnregisteredToken)]
    [InlineData(MessagingErrorCode.SenderIdMismatch, FirebaseErrorCodes.SenderIdMismatch)]
    [InlineData(MessagingErrorCode.QuotaExceeded, MessagingErrorCodes.RateLimitExceeded)]
    [InlineData(MessagingErrorCode.ThirdPartyAuthError, FirebaseErrorCodes.ThirdPartyAuthError)]
    [InlineData(MessagingErrorCode.Unavailable, FirebaseErrorCodes.ServiceUnavailable)]
    [InlineData(MessagingErrorCode.Internal, FirebaseErrorCodes.InternalError)]
    public void Should_MapFirebaseErrorCode_When_KnownCode(MessagingErrorCode input, string expected)
    {
        var result = FirebaseService.MapFirebaseErrorCode(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_MapFirebaseErrorCode_When_Null()
    {
        var result = FirebaseService.MapFirebaseErrorCode(null);
        Assert.Equal(MessagingErrorCodes.SendMessageFailed, result);
    }

    [Fact]
    public void Should_MapFirebaseErrorCode_When_UnknownCode()
    {
        var result = FirebaseService.MapFirebaseErrorCode((MessagingErrorCode)999);
        Assert.Equal(MessagingErrorCodes.SendMessageFailed, result);
    }
}
