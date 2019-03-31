using System;

namespace ISimpleSocket.Events
{
	public sealed class ServerStartFailedEventArgs : EventArgs
	{
		public Exception Exception { get; }

		public ServerStartFailedEventArgs(Exception exception)
		{
			Exception = exception;
		}
	}
}
