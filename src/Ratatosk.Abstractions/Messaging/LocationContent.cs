//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json.Serialization;

namespace Ratatosk
{
	/// <summary>
	/// Represents message content that contains geographical location data.
	/// </summary>
	/// <remarks>
	/// This implementation provides location information including latitude and longitude coordinates,
	/// and optionally additional location-specific properties such as accuracy, live period,
	/// heading, and proximity alert radius.
	/// </remarks>
	public class LocationContent : MessageContent, ILocationContent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocationContent"/> class with the specified coordinates.
		/// </summary>
		/// <param name="latitude">The latitude coordinate in decimal degrees.</param>
		/// <param name="longitude">The longitude coordinate in decimal degrees.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when latitude is not between -90 and 90, or longitude is not between -180 and 180.
		/// </exception>
		public LocationContent(double latitude, double longitude)
		{
			if (latitude < -90 || latitude > 90)
				throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90 degrees.");
			
			if (longitude < -180 || longitude > 180)
				throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180 degrees.");

			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocationContent"/> class from an existing location content.
		/// </summary>
		/// <param name="content">The existing location content to copy from.</param>
		/// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
		public LocationContent(ILocationContent content)
		{
			if (content == null)
				throw new ArgumentNullException(nameof(content));

			Latitude = content.Latitude;
			Longitude = content.Longitude;
			HorizontalAccuracy = content.HorizontalAccuracy;
			LivePeriod = content.LivePeriod;
			Heading = content.Heading;
			ProximityAlertRadius = content.ProximityAlertRadius;
		}

		/// <inheritdoc/>
		public override MessageContentType ContentType => MessageContentType.Location;

		/// <inheritdoc/>
		[JsonPropertyName("latitude")]
		public double Latitude { get; }

		/// <inheritdoc/>
		[JsonPropertyName("longitude")]
		public double Longitude { get; }

		/// <inheritdoc/>
		[JsonPropertyName("horizontal_accuracy")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public double? HorizontalAccuracy { get; set; }

		/// <inheritdoc/>
		[JsonPropertyName("live_period")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? LivePeriod { get; set; }

		/// <inheritdoc/>
		[JsonPropertyName("heading")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? Heading { get; set; }

		/// <inheritdoc/>
		[JsonPropertyName("proximity_alert_radius")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? ProximityAlertRadius { get; set; }

		/// <summary>
		/// Sets the horizontal accuracy of the location.
		/// </summary>
		/// <param name="accuracy">The accuracy in meters.</param>
		/// <returns>The current instance for method chaining.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when accuracy is negative.</exception>
		public LocationContent WithHorizontalAccuracy(double accuracy)
		{
			if (accuracy < 0)
				throw new ArgumentOutOfRangeException(nameof(accuracy), accuracy, "Accuracy cannot be negative.");

			HorizontalAccuracy = accuracy;
			return this;
		}

		/// <summary>
		/// Sets the live period for the location.
		/// </summary>
		/// <param name="livePeriod">The live period in seconds.</param>
		/// <returns>The current instance for method chaining.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when livePeriod is not between 60 and 86400 seconds.</exception>
		public LocationContent WithLivePeriod(int livePeriod)
		{
			if (livePeriod < 60 || livePeriod > 86400)
				throw new ArgumentOutOfRangeException(nameof(livePeriod), livePeriod, "Live period must be between 60 and 86400 seconds.");

			LivePeriod = livePeriod;
			return this;
		}

		/// <summary>
		/// Sets the heading direction for the location.
		/// </summary>
		/// <param name="heading">The heading direction in degrees (1-360).</param>
		/// <returns>The current instance for method chaining.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when heading is not between 1 and 360 degrees.</exception>
		public LocationContent WithHeading(int heading)
		{
			if (heading < 1 || heading > 360)
				throw new ArgumentOutOfRangeException(nameof(heading), heading, "Heading must be between 1 and 360 degrees.");

			Heading = heading;
			return this;
		}

		/// <summary>
		/// Sets the proximity alert radius for the location.
		/// </summary>
		/// <param name="radius">The proximity alert radius in meters.</param>
		/// <returns>The current instance for method chaining.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when radius is not between 1 and 100000 meters.</exception>
		public LocationContent WithProximityAlertRadius(int radius)
		{
			if (radius < 1 || radius > 100000)
				throw new ArgumentOutOfRangeException(nameof(radius), radius, "Proximity alert radius must be between 1 and 100000 meters.");

			ProximityAlertRadius = radius;
			return this;
		}

		/// <summary>
		/// Returns a string representation of the location.
		/// </summary>
		/// <returns>A string containing the latitude and longitude coordinates.</returns>
		public override string ToString()
		{
			return $"Location({Latitude:F6}, {Longitude:F6})";
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current location.
		/// </summary>
		/// <param name="obj">The object to compare with the current location.</param>
		/// <returns>True if the specified object is equal to the current location; otherwise, false.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is not LocationContent other)
				return false;

			return Math.Abs(Latitude - other.Latitude) < 1e-9 &&
				   Math.Abs(Longitude - other.Longitude) < 1e-9 &&
				   HorizontalAccuracy == other.HorizontalAccuracy &&
				   LivePeriod == other.LivePeriod &&
				   Heading == other.Heading &&
				   ProximityAlertRadius == other.ProximityAlertRadius;
		}

		/// <summary>
		/// Returns a hash code for the current location.
		/// </summary>
		/// <returns>A hash code for the current location.</returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(Latitude, Longitude, HorizontalAccuracy, LivePeriod, Heading, ProximityAlertRadius);
		}
	}
}