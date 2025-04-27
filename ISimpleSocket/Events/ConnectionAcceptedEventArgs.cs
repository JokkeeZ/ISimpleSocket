using System.Net.Sockets;

namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event,
/// which occurs when connection was accepted.
/// </summary>
/// <param name="connectionId">Accepted connection id.</param>
/// <param name="socket">Accepted connection socket.</param>
public sealed class ConnectionAcceptedEventArgs(int connectionId, Socket socket) : EventArgs
{
    /// <summary>
    /// Accepted connection id.
    /// </summary>
    public int ConnectionId { get; } = connectionId;

    /// <summary>
    /// Accepted connection socket.
    /// </summary>
    public Socket Socket { get; } = socket;
}