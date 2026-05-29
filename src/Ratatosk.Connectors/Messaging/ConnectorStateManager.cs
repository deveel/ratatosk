//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Default thread-safe implementation of <see cref="IConnectorStateManager"/>
	/// that serialises state transitions with a lock.
	/// </summary>
	public sealed class ConnectorStateManager : IConnectorStateManager
	{
		private ConnectorState _state = ConnectorState.Uninitialized;
		private readonly object _lock = new();

		/// <inheritdoc/>
		public ConnectorState Current
		{
			get { lock (_lock) { return _state; } }
		}

		/// <inheritdoc/>
		public void TransitionTo(ConnectorState newState)
		{
			lock (_lock)
			{
				_state = newState;
			}
		}

		/// <inheritdoc/>
		public void EnsureOperational()
		{
			var state = Current;
			if (state == ConnectorState.Uninitialized ||
				state == ConnectorState.Initializing ||
				state == ConnectorState.ShuttingDown ||
				state == ConnectorState.Shutdown)
			{
				throw new MessagingException(
					MessagingErrorCodes.MessagingError,
					MessagingErrorCodes.ErrorDomain,
					$"The connector is not in an operational state. Current state: {state}");
			}
		}
	}
}
