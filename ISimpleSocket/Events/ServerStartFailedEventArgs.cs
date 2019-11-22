using System;

namespace ISimpleSocket.Events
{
	/// <summary>
	/// Represents event arguments for event, which occurs when server start has failed.
	/// </summary>
	public sealed class ServerStartFailedEventArgs : EventArgs
	{
		/// <summary>
		/// Exception that occurred during server start.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Initializes an new instance of <see cref="ServerStartFailedEventArgs"/> 
		/// with exception, that occurred during server start.
		/// </summary>
		/// <param name="exception"></param>
		public ServerStartFailedEventArgs(Exception exception)
		{
			Exception = exception;
		}
	}
}
