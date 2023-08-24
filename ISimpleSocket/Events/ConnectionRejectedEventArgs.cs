using System.Net.Sockets;

namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event, which occurs when connection was rejected by the server.
/// </summary>
public sealed class ConnectionRejectedEventArgs
{
	/// <summary>
	/// Rejected connection socket.
	/// </summary>
	public Socket Socket { get; }

	/// <summary>
	/// Initializes an new instance of <see cref="ConnectionRejectedEventArgs"/> with rejected socket.
	/// </summary>
	/// <param name="socket">Rejected connection socket</param>
	public ConnectionRejectedEventArgs(Socket socket) => Socket = socket;
}
