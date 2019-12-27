using System;
using System.Net.Sockets;

namespace ISimpleSocket.Events
{
	/// <summary>
	/// Represents event arguments for event, which occurs when server start has failed.
	/// </summary>
	public sealed class ServerStartFailedEventArgs : EventArgs
	{
		/// <summary>
		/// Exception socker error code that came during server start.
		/// </summary>
		public SocketError ErrorCode { get; }

		/// <summary>
		/// Initializes an new instance of <see cref="ServerStartFailedEventArgs"/> 
		/// with error code, that occurred during server start.
		/// </summary>
		/// <param name="error">Error code, that occurred during server start.</param>
		public ServerStartFailedEventArgs(SocketError error) => ErrorCode = error;
	}
}
