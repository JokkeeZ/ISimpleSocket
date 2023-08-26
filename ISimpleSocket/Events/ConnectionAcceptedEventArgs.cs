using System.Net.Sockets;

namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event, which occurs when connection was accepted.
/// </summary>
public sealed class ConnectionAcceptedEventArgs : EventArgs
{
	/// <summary>
	/// Accepted connection id.
	/// </summary>
	public int ConnectionId { get; }

	/// <summary>
	/// Accepted connection socket.
	/// </summary>
	public Socket Socket { get; }

	/// <summary>
	/// Initializes an new instance of <see cref="ConnectionAcceptedEventArgs"/> with connection id and socket.
	/// </summary>
	/// <param name="connectionId">Accepted connection id.</param>
	/// <param name="socket">Accepted connection socket.</param>
	public ConnectionAcceptedEventArgs(int connectionId, Socket socket)
		=> (ConnectionId, Socket) = (connectionId, socket);
}
