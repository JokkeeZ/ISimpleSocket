namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event, which occurs when server start has failed.
/// </summary>
/// <remarks>
/// Initializes an new instance of <see cref="ServerStartFailedEventArgs"/> 
/// with <see cref="System.Exception"/>, that occurred during server start.
/// </remarks>
/// <param name="exception"><see cref="System.Exception"/>, that occurred during server start.</param>
public sealed class ServerStartFailedEventArgs(Exception exception) : EventArgs
{
	/// <summary>
	/// Exception that occured during server start.
	/// </summary>
	public Exception Exception { get; } = exception;
}
