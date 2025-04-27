namespace ISimpleSocket.Events;

/// <summary>
/// Represents event arguments for event, which occurs when server start has failed.
/// </summary>
/// <param name="ex"><see cref="System.Exception"/>, that occurred during server start.</param>
public sealed class ServerStartFailedEventArgs(Exception ex) : EventArgs
{
    /// <summary>
    /// Exception that occurred during server start.
    /// </summary>
    public Exception Exception { get; } = ex;
}