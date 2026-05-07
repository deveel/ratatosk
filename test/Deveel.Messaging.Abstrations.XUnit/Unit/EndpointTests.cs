namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Endpoint")]
public class EndpointTests
{
    [Fact]
    public void Should_CreateEmptyEndpoint_When_EndpointDefaultConstructor()
    {
        // Arrange
        // Act
        var endpoint = new Endpoint();

        // Assert
        Assert.Equal(new EndpointType(), endpoint.Type);
        Assert.Equal("", endpoint.Address);
    }

    [Fact]
    public void Should_SetProperties_When_EndpointConstructorWithTypeAndAddress()
    {
        // Arrange
        var type = EndpointType.EmailAddress;
        var address = "test@example.com";

        // Act
        var endpoint = new Endpoint(type, address);

        // Assert
        Assert.Equal(type, endpoint.Type);
        Assert.Equal(address, endpoint.Address);
    }

    [Fact]
    public void Should_CopiesProperties_When_EndpointConstructorWithIEndpoint()
    {
        // Arrange
        var sourceEndpoint = new Endpoint(EndpointType.PhoneNumber, "+1234567890");

        // Act
        var endpoint = new Endpoint(sourceEndpoint);

        // Assert
        Assert.Equal(EndpointType.PhoneNumber, endpoint.Type);
        Assert.Equal("+1234567890", endpoint.Address);
    }

    [Fact]
    public void Should_ReturnEndpoint_When_CreateValidTypeAndAddress()
    {
        // Arrange
        var type = EndpointType.EmailAddress;
        var address = "test@example.com";

        // Act
        var endpoint = Endpoint.Create(type, address);

        // Assert
        Assert.Equal(type, endpoint.Type);
        Assert.Equal(address, endpoint.Address);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_ThrowArgumentException_When_CreateInvalidEmptyType(string? type)
    {
        // Arrange
        var address = "test@example.com";

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Endpoint.Create(type!, address));
    }

	[Fact]
	public void Should_ThrowArgumentNullException_When_CreateInvalidNullType()
	{
		// Arrange
		var address = "test@example.com";

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => Endpoint.Create((string)null!, address));
	}


	[Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_ThrowArgumentException_When_CreateInvalidEmptyAddress(string? address)
    {
        // Arrange
        var type = EndpointType.EmailAddress;

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Endpoint.Create(type, address!));
    }

    [Fact]
	public void Should_ThrowArgumentException_When_CreateInvalidNullAddress()
	{
		// Arrange
		var type = EndpointType.EmailAddress;

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => Endpoint.Create(type, null!));
	}


	[Fact]
    public void Should_ReturnEndpointWithIdType_When_IdValidEndpointId()
    {
        // Arrange
        var endpointId = "endpoint-123";

        // Act
        var endpoint = Endpoint.Id(endpointId);

        // Assert
        Assert.Equal(EndpointType.Id, endpoint.Type);
        Assert.Equal(endpointId, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnEmailEndpoint_When_EmailAddressValidEmail()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var endpoint = Endpoint.EmailAddress(email);

        // Assert
        Assert.Equal(EndpointType.EmailAddress, endpoint.Type);
        Assert.Equal(email, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnPhoneEndpoint_When_PhoneNumberValidPhone()
    {
        // Arrange
        var phone = "+1234567890";

        // Act
        var endpoint = Endpoint.PhoneNumber(phone);

        // Assert
        Assert.Equal(EndpointType.PhoneNumber, endpoint.Type);
        Assert.Equal(phone, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnUrlEndpoint_When_UrlValidUrl()
    {
        // Arrange
        var url = "https://example.com/webhook";

        // Act
        var endpoint = Endpoint.Url(url);

        // Assert
        Assert.Equal(EndpointType.Url, endpoint.Type);
        Assert.Equal(url, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnApplicationEndpoint_When_ApplicationValidAppId()
    {
        // Arrange
        var appId = "app-123";

        // Act
        var endpoint = Endpoint.Application(appId);

        // Assert
        Assert.Equal(EndpointType.ApplicationId, endpoint.Type);
        Assert.Equal(appId, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnUserEndpoint_When_UserValidUserId()
    {
        // Arrange
        var userId = "user-456";

        // Act
        var endpoint = Endpoint.User(userId);

        // Assert
        Assert.Equal(EndpointType.UserId, endpoint.Type);
        Assert.Equal(userId, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnDeviceEndpoint_When_DeviceValidDeviceId()
    {
        // Arrange
        var deviceId = "device-789";

        // Act
        var endpoint = Endpoint.Device(deviceId);

        // Assert
        Assert.Equal(EndpointType.DeviceId, endpoint.Type);
        Assert.Equal(deviceId, endpoint.Address);
    }

    [Fact]
    public void Should_ReturnLabelEndpoint_When_AlphaNumericValidLabel()
    {
        // Arrange
        var label = "TEST123";

        // Act
        var endpoint = Endpoint.AlphaNumeric(label);

        // Assert
        Assert.Equal(EndpointType.Label, endpoint.Type);
        Assert.Equal(label, endpoint.Address);
    }

    [Fact]
    public void Should_ExposeCorrectProperties_When_IEndpointImplementation()
    {
        // Arrange
        var endpoint = new Endpoint(EndpointType.EmailAddress, "test@example.com");

        // Act
        // Assert
        IEndpoint iEndpoint = endpoint;
        Assert.Equal(EndpointType.EmailAddress, iEndpoint.Type);
        Assert.Equal("test@example.com", iEndpoint.Address);
    }

    [Fact]
    public void Should_UpdateProperties_When_PropertySettersSetValues()
    {
        // Arrange
        var endpoint = new Endpoint();

        // Act
        endpoint.Type = EndpointType.PhoneNumber;
        endpoint.Address = "+9876543210";

        // Assert
        Assert.Equal(EndpointType.PhoneNumber, endpoint.Type);
        Assert.Equal("+9876543210", endpoint.Address);
    }

    [Fact]
    public void Should_ConvertToEnum_When_EndpointConstructorWithStringType()
    {
        // Arrange
        // Act
        var endpoint = new Endpoint("email", "test@example.com");

        // Assert
        Assert.Equal(EndpointType.EmailAddress, endpoint.Type);
        Assert.Equal("test@example.com", endpoint.Address);
    }

    [Theory]
    [InlineData("email", EndpointType.EmailAddress)]
    [InlineData("phone", EndpointType.PhoneNumber)]
    [InlineData("url", EndpointType.Url)]
    [InlineData("user-id", EndpointType.UserId)]
    [InlineData("app-id", EndpointType.ApplicationId)]
    [InlineData("endpoint-id", EndpointType.Id)]
    [InlineData("device-id", EndpointType.DeviceId)]
    [InlineData("label", EndpointType.Label)]
    public void Should_ConvertCorrectly_When_EndpointConstructorWithStringTypes(string stringType, EndpointType expectedType)
    {
        // Arrange
        var address = "test-address";

        // Act
        var endpoint = new Endpoint(stringType, address);

        // Assert
        Assert.Equal(expectedType, endpoint.Type);
        Assert.Equal(address, endpoint.Address);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_EndpointConstructorWithUnknownStringType()
    {
        // Arrange
        var unknownType = "unknown-type";
        var address = "test-address";

        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => new Endpoint(unknownType, address));
        Assert.Contains("Unknown endpoint type: unknown-type", exception.Message);
    }

    [Fact]
    public void Should_ConvertToEnum_When_CreateWithStringType()
    {
        // Arrange
        var type = "email";
        var address = "test@example.com";

        // Act
        var endpoint = Endpoint.Create(type, address);

        // Assert
        Assert.Equal(EndpointType.EmailAddress, endpoint.Type);
        Assert.Equal(address, endpoint.Address);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CreateWithUnknownStringType()
    {
        // Arrange
        var unknownType = "unknown";
        var address = "test-address";

        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => Endpoint.Create(unknownType, address));
        Assert.Contains("Unknown endpoint type: unknown", exception.Message);
    }

    [Theory]
    [InlineData("EMAIL")]
    [InlineData("Email")]
    [InlineData("PHONE")]
    [InlineData("Phone")]
    public void Should_BeCaseInsensitive_When_EndpointStringTypeConversion(string stringType)
    {
        // Arrange
        var address = "test-address";

        // Act
        var endpoint = new Endpoint(stringType, address);

        // Assert
        Assert.True(endpoint.Type == EndpointType.EmailAddress || endpoint.Type == EndpointType.PhoneNumber);
        Assert.Equal(address, endpoint.Address);
    }
}