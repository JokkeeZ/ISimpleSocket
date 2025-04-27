namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has disconnected.
/// </summary>
/// <param name="connection">Connection that was disconnected</param>
public sealed class ConnectionClosedEventArgs(ISimpleConnection connection) : EventArgs
{
    /// <summary>
    /// Connection that was disconnected.
    /// </summary>
    public ISimpleConnection Connection { get; } = connection;
}