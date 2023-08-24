namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event, which occurs when server start has failed.
/// </summary>
public sealed class ServerStartFailedEventArgs : EventArgs
{
	/// <summary>
	/// Exception that occured during server start.
	/// </summary>
	public Exception Exception { get; }

	/// <summary>
	/// Initializes an new instance of <see cref="ServerStartFailedEventArgs"/> 
	/// with <see cref="System.Exception"/>, that occurred during server start.
	/// </summary>
	/// <param name="exception"><see cref="System.Exception"/>, that occurred during server start.</param>
	public ServerStartFailedEventArgs(Exception exception) => Exception = exception;
}
