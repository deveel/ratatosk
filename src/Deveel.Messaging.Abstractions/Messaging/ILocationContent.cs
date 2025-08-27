//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Defines the contract for message content that represents a geographical location.
	/// </summary>
	/// <remarks>
	/// This interface provides access to location data including coordinates and 
	/// optional location-specific properties that can be used by messaging systems
	/// that support location sharing, such as Telegram.
	/// </remarks>
	public interface ILocationContent : IMessageContent
	{
		/// <summary>
		/// Gets the latitude coordinate in decimal degrees.
		/// </summary>
		/// <remarks>
		/// Latitude values range from -90 to 90 degrees, where negative values 
		/// represent locations south of the equator.
		/// </remarks>
		double Latitude { get; }

		/// <summary>
		/// Gets the longitude coordinate in decimal degrees.
		/// </summary>
		/// <remarks>
		/// Longitude values range from -180 to 180 degrees, where negative values 
		/// represent locations west of the Prime Meridian.
		/// </remarks>
		double Longitude { get; }

		/// <summary>
		/// Gets the accuracy of the location in meters.
		/// </summary>
		/// <remarks>
		/// This optional property indicates the accuracy radius of the location.
		/// A null value indicates that accuracy is not specified.
		/// </remarks>
		double? HorizontalAccuracy { get; }

		/// <summary>
		/// Gets the period in seconds for which the location can be updated.
		/// </summary>
		/// <remarks>
		/// This is used for live locations that can be updated in real-time.
		/// A null value indicates a static location.
		/// </remarks>
		int? LivePeriod { get; }

		/// <summary>
		/// Gets the direction in which the user is moving, in degrees.
		/// </summary>
		/// <remarks>
		/// Heading values range from 1 to 360 degrees, where 1 represents north.
		/// A null value indicates that heading is not specified.
		/// </remarks>
		int? Heading { get; }

		/// <summary>
		/// Gets the maximum distance for proximity alerts about approaching the location, in meters.
		/// </summary>
		/// <remarks>
		/// This is used to set up proximity alerts when other users come within 
		/// the specified distance of this location.
		/// </remarks>
		int? ProximityAlertRadius { get; }
	}
}