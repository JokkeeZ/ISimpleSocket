using System;

namespace ISimpleSocket.Client.Events
{
	/// <summary>
	/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> is sending data.
	/// </summary>
	public sealed class ConnectionSendingDataEventArgs : EventArgs
	{
		private readonly byte[] data;

		/// <summary>
		/// Initializes an new instance of the <see cref="ConnectionSendingDataEventArgs"/> with data to be sent.
		/// </summary>
		/// <param name="data">Data to be sent.</param>
		public ConnectionSendingDataEventArgs(byte[] data) => this.data = data;

		/// <summary>
		/// Gets shallow copy of data that will be sent.
		/// </summary>
		/// <returns>Shallow copy of data that will be sent.</returns>
		public byte[] GetData() => (byte[])data.Clone();
	}
}
