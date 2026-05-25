//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Xunit;

namespace Ratatosk
{
	/// <summary>
	/// Tests for LocationContent functionality.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Infrastructure")]
	[Trait("Feature", "LocationContent")]
	public class LocationContentTests
	{
		[Fact]
		public void Should_SetProperties_When_LocationContentWithValidCoordinates()
		{
			// Arrange
			// Act
			var location = new LocationContent(40.7128, -74.0060);

			// Assert
			Assert.Equal(40.7128, location.Latitude);
			Assert.Equal(-74.0060, location.Longitude);
			Assert.Equal(MessageContentType.Location, location.ContentType);
			Assert.Null(location.HorizontalAccuracy);
			Assert.Null(location.LivePeriod);
			Assert.Null(location.Heading);
			Assert.Null(location.ProximityAlertRadius);
		}

		[Theory]
		[InlineData(-91, 0)]
		[InlineData(91, 0)]
		[InlineData(0, -181)]
		[InlineData(0, 181)]
		public void Should_ThrowArgumentOutOfRangeException_When_LocationContentWithInvalidCoordinates(double latitude, double longitude)
		{
			// Act
			// Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => new LocationContent(latitude, longitude));
		}

		[Fact]
		public void Should_SetAllProperties_When_LocationContentWithFluentConfiguration()
		{
			// Arrange
			// Act
			var location = new LocationContent(51.5074, -0.1278) // London coordinates
				.WithHorizontalAccuracy(10.5)
				.WithLivePeriod(3600)
				.WithHeading(90)
				.WithProximityAlertRadius(500);

			// Assert
			Assert.Equal(51.5074, location.Latitude);
			Assert.Equal(-0.1278, location.Longitude);
			Assert.Equal(10.5, location.HorizontalAccuracy);
			Assert.Equal(3600, location.LivePeriod);
			Assert.Equal(90, location.Heading);
			Assert.Equal(500, location.ProximityAlertRadius);
		}

		[Theory]
		[InlineData(-1)]
		public void Should_ThrowArgumentOutOfRangeException_When_LocationContentWithInvalidHorizontalAccuracy(double accuracy)
		{
			// Arrange
			var location = new LocationContent(0, 0);

			// Act
			// Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => location.WithHorizontalAccuracy(accuracy));
		}

		[Theory]
		[InlineData(59)]
		[InlineData(86401)]
		public void Should_ThrowArgumentOutOfRangeException_When_LocationContentWithInvalidLivePeriod(int livePeriod)
		{
			// Arrange
			var location = new LocationContent(0, 0);

			// Act
			// Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => location.WithLivePeriod(livePeriod));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(361)]
		public void Should_ThrowArgumentOutOfRangeException_When_LocationContentWithInvalidHeading(int heading)
		{
			// Arrange
			var location = new LocationContent(0, 0);

			// Act
			// Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => location.WithHeading(heading));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(100001)]
		public void Should_ThrowArgumentOutOfRangeException_When_LocationContentWithInvalidProximityAlertRadius(int radius)
		{
			// Arrange
			var location = new LocationContent(0, 0);

			// Act
			// Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => location.WithProximityAlertRadius(radius));
		}

		[Fact]
		public void Should_ReturnFormattedString_When_LocationContentToString()
		{
			// Arrange
			var location = new LocationContent(40.7128, -74.0060);

			// Act
			var result = location.ToString();

			// Assert
			Assert.Equal("Location(40.712800, -74.006000)", result);
		}

		[Fact]
		public void Should_ReturnTrue_When_LocationContentEqualsWithSameValues()
		{
			// Arrange
			var location1 = new LocationContent(40.7128, -74.0060)
				.WithLivePeriod(3600)
				.WithHeading(45);
			var location2 = new LocationContent(40.7128, -74.0060)
				.WithLivePeriod(3600)
				.WithHeading(45);

			// Act
			// Assert
			Assert.True(location1.Equals(location2));
			Assert.Equal(location1.GetHashCode(), location2.GetHashCode());
		}

		[Fact]
		public void Should_ReturnFalse_When_LocationContentEqualsWithDifferentValues()
		{
			// Arrange
			var location1 = new LocationContent(40.7128, -74.0060);
			var location2 = new LocationContent(51.5074, -0.1278);

			// Act
			// Assert
			Assert.False(location1.Equals(location2));
		}

		[Fact]
		public void Should_CopiesAllProperties_When_LocationContentCopyConstructor()
		{
			// Arrange
			var original = new LocationContent(40.7128, -74.0060)
				.WithHorizontalAccuracy(15.0)
				.WithLivePeriod(1800)
				.WithHeading(180)
				.WithProximityAlertRadius(1000);

			// Act
			var copy = new LocationContent(original);

			// Assert
			Assert.Equal(original.Latitude, copy.Latitude);
			Assert.Equal(original.Longitude, copy.Longitude);
			Assert.Equal(original.HorizontalAccuracy, copy.HorizontalAccuracy);
			Assert.Equal(original.LivePeriod, copy.LivePeriod);
			Assert.Equal(original.Heading, copy.Heading);
			Assert.Equal(original.ProximityAlertRadius, copy.ProximityAlertRadius);
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_LocationContentCopyConstructorWithNull()
		{
			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => new LocationContent(null!));
		}
	}
}