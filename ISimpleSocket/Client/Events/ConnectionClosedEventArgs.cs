using System;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionClosedEventArgs : EventArgs
	{
		public ISimpleConnection Connection { get; }

		public ConnectionClosedEventArgs(ISimpleConnection connection)
		{
			Connection = connection;
		}
	}
}
