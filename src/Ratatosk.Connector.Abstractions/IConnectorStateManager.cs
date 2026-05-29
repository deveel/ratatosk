//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Manages the lifecycle state of a channel connector in a thread-safe manner.
	/// </summary>
	/// <remarks>
	/// Implementations must guarantee that state reads and transitions
	/// are atomic with respect to concurrent callers. The default implementation
	/// uses a simple lock; custom implementations may use lighter-weight primitives.
	/// </remarks>
	public interface IConnectorStateManager
	{
		/// <summary>
		/// Gets the current state of the connector.
		/// </summary>
		ConnectorState Current { get; }

		/// <summary>
		/// Transitions the connector to <paramref name="newState"/>.
		/// </summary>
		/// <param name="newState">The target state.</param>
		void TransitionTo(ConnectorState newState);

		/// <summary>
		/// Throws a <see cref="MessagingException"/> if the connector is not
		/// in an operational state (i.e. not <see cref="ConnectorState.Ready"/>
		/// or <see cref="ConnectorState.Error"/>).
		/// </summary>
		/// <exception cref="MessagingException">
		/// Thrown when the connector is in a non-operational state such as
		/// <see cref="ConnectorState.Uninitialized"/>, <see cref="ConnectorState.Initializing"/>,
		/// <see cref="ConnectorState.ShuttingDown"/>, or <see cref="ConnectorState.Shutdown"/>.
		/// </exception>
		void EnsureOperational();
	}
}
