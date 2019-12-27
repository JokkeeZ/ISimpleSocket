using System;

namespace ISimpleSocket.Client.Events
{
	/// <summary>
	/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has received data.
	/// </summary>
	public sealed class ConnectionReceivedDataEventArgs : EventArgs
	{
		private readonly byte[] data;

		/// <summary>
		/// Initializes an new instance of the <see cref="ConnectionReceivedDataEventArgs"/>, with data that was received.
		/// </summary>
		/// <param name="data">Data that was received</param>
		public ConnectionReceivedDataEventArgs(byte[] data) => this.data = data;

		/// <summary>
		/// Gets a shallow copy of data that was received.
		/// </summary>
		/// <returns>Shallow copy of data that was received.</returns>
		public byte[] GetReceivedData() => (byte[])data.Clone();
	}
}
