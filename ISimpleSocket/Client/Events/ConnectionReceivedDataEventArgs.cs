using System;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionReceivedDataEventArgs : EventArgs
	{
		private readonly byte[] _receivedData;

		public ConnectionReceivedDataEventArgs(byte[] data)
		{
			_receivedData = data;
		}

		public byte[] GetReceivedData() => (byte[])_receivedData.Clone();
	}
}
