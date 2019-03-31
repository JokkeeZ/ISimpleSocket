using System;
using System.Net.Sockets;

namespace ISimpleSocket.Events
{
	public sealed class ConnectionReceivedEventArgs : EventArgs
	{
		public int ConnectionId { get; }

		public Socket Socket { get; }

		public ConnectionReceivedEventArgs(int connectionId, Socket socket)
		{
			ConnectionId = connectionId;
			Socket = socket;
		}
	}
}
