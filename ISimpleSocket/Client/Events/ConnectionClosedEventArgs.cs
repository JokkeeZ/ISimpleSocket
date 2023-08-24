namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has disconnected.
/// </summary>
public sealed class ConnectionClosedEventArgs : EventArgs
{
	/// <summary>
	/// Connection that was disconnected.
	/// </summary>
	public ISimpleConnection Connection { get; }

	/// <summary>
	/// Initializes an new instance of the <see cref="ConnectionClosedEventArgs"/>,
	/// with connection that was disconnected.
	/// </summary>
	/// <param name="connection">Connection that was disconnected</param>
	public ConnectionClosedEventArgs(ISimpleConnection connection) => Connection = connection;
}
