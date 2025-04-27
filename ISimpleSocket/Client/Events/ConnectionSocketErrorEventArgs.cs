using System.Net.Sockets;

namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event,
/// which occurs when <see cref="SimpleConnection"/> has socket error.
/// </summary>
/// <param name="error">Error that occurred</param>
public sealed class ConnectionSocketErrorEventArgs(SocketError error) : EventArgs
{
    /// <summary>
    /// Gets a value of the <see cref="SocketError"/> that occurred.
    /// </summary>
    public SocketError Error { get; } = error;
}