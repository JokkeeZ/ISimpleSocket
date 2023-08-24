using System.Net.Sockets;

namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has socket error.
/// </summary>
public sealed class ConnectionSocketErrorEventArgs : EventArgs
{
	/// <summary>
	/// Gets a value of the <see cref="SocketError"/> that occurred.
	/// </summary>
	public SocketError Error { get; }

	/// <summary>
	/// Initializes an new instance of the <see cref="ConnectionSocketErrorEventArgs"/> 
	/// with <see cref="SocketError"/> that occurred.
	/// </summary>
	/// <param name="error"></param>
	public ConnectionSocketErrorEventArgs(SocketError error) => Error = error;
}
