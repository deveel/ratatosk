using FirebaseAdmin.Messaging;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FirebaseService")]
public class FirebaseServiceMappingTests
{
    [Theory]
    [InlineData(null, MessagingErrorCodes.SendMessageFailed)]
    [InlineData(MessagingErrorCode.InvalidArgument, FirebaseErrorCodes.InvalidArgument)]
    [InlineData(MessagingErrorCode.Unregistered, FirebaseErrorCodes.UnregisteredToken)]
    [InlineData(MessagingErrorCode.SenderIdMismatch, FirebaseErrorCodes.SenderIdMismatch)]
    [InlineData(MessagingErrorCode.QuotaExceeded, MessagingErrorCodes.RateLimitExceeded)]
    [InlineData(MessagingErrorCode.ThirdPartyAuthError, FirebaseErrorCodes.ThirdPartyAuthError)]
    [InlineData(MessagingErrorCode.Unavailable, FirebaseErrorCodes.ServiceUnavailable)]
    [InlineData(MessagingErrorCode.Internal, FirebaseErrorCodes.InternalError)]
    [InlineData((MessagingErrorCode)999, MessagingErrorCodes.SendMessageFailed)]
    public void Should_MapCorrectly_When_FirebaseErrorCodeProvided(MessagingErrorCode? code, string expected)
    {
        var result = FirebaseService.MapFirebaseErrorCode(code);
        Assert.Equal(expected, result);
    }
}
