using System;

namespace ISimpleSocket.Client.Events
{
	public sealed class ConnectionSendingDataEventArgs : EventArgs
	{
		private readonly byte[] _data;

		public ConnectionSendingDataEventArgs(byte[] data)
		{
			_data = data;
		}

		public byte[] GetData() => (byte[])_data.Clone();
	}
}
