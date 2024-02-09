namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has disconnected.
/// </summary>
/// <remarks>
/// Initializes an new instance of the <see cref="ConnectionClosedEventArgs"/>,
/// with connection that was disconnected.
/// </remarks>
/// <param name="connection">Connection that was disconnected</param>
public sealed class ConnectionClosedEventArgs(ISimpleConnection connection) : EventArgs
{
	/// <summary>
	/// Connection that was disconnected.
	/// </summary>
	public ISimpleConnection Connection { get; } = connection;
}
