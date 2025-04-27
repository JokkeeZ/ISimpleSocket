using System.Net.Sockets;

namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event,
/// which occurs when connection was rejected by the server.
/// </summary>
/// <param name="socket">Rejected connection socket</param>
public sealed class ConnectionRejectedEventArgs(Socket socket) : EventArgs
{
    /// <summary>
    /// Rejected connection socket.
    /// </summary>
    public Socket Socket { get; } = socket;
}