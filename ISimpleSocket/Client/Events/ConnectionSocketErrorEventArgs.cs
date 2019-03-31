using System;
using System.Net.Sockets;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionSocketErrorEventArgs : EventArgs
	{
		public SocketError Error { get; }

		public ConnectionSocketErrorEventArgs(SocketError error)
		{
			Error = error;
		}
	}
}
