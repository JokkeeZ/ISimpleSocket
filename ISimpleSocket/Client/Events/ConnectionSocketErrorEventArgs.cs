namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has socket error.
/// </summary>
/// <remarks>
/// Initializes an new instance of the <see cref="ConnectionSocketErrorEventArgs"/> 
/// with <see cref="SocketError"/> that occurred.
/// </remarks>
/// <param name="error"></param>
public sealed class ConnectionSocketErrorEventArgs(SocketError error) : EventArgs
{
	/// <summary>
	/// Gets a value of the <see cref="SocketError"/> that occurred.
	/// </summary>
	public SocketError Error { get; } = error;
}
