using System;
using System.Net.Sockets;

namespace ISimpleSocket.Events
{
	/// <summary>
	/// Represents event arguments for event, which occurs when connection was received.
	/// </summary>
	public sealed class ConnectionReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Received connection id.
		/// </summary>
		public int ConnectionId { get; }

		/// <summary>
		/// Received connection socket.
		/// </summary>
		public Socket Socket { get; }

		/// <summary>
		/// Initializes an new instance of <see cref="ConnectionReceivedEventArgs"/> with connection id and socket.
		/// </summary>
		/// <param name="connectionId">Received connection id.</param>
		/// <param name="socket">Received connection socket.</param>
		public ConnectionReceivedEventArgs(int connectionId, Socket socket)
		{
			ConnectionId = connectionId;
			Socket = socket;
		}
	}
}
