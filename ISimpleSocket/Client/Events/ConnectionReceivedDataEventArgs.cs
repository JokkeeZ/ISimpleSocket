using System;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionReceivedDataEventArgs : EventArgs
	{
		public byte[] ReceivedData { get; }

		public ConnectionReceivedDataEventArgs(byte[] data)
		{
			ReceivedData = data;
		}
	}
}
