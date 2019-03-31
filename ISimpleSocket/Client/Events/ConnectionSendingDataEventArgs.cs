using System;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionSendingDataEventArgs : EventArgs
	{
		public byte[] Data { get; }

		public ConnectionSendingDataEventArgs(byte[] data)
		{
			Data = data;
		}
	}
}
