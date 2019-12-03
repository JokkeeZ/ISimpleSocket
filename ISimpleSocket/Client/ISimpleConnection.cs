using System;

namespace ISimpleSocket.Client
{
	/// <summary>
	/// Represents an connection for <see cref="SimpleServer"/> instance.
	/// </summary>
	public interface ISimpleConnection : IDisposable
	{
		/// <summary>
		/// Unique connection id.
		/// </summary>
		[Obsolete("ConnectionId is deprecated, please use Id instead.")]
		int ConnectionId { get; }

		/// <summary>
		/// Unique connection id.
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Gets a value that indicates, if connection is disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Gets a value that indicates, if connection is connected to the server.
		/// </summary>
		bool Connected { get; }

		/// <summary>
		/// Sends data to the server.
		/// </summary>
		/// <param name="data">Data to be sent.</param>
		void SendData(byte[] data);

		/// <summary>
		/// Disconnects from the server.
		/// </summary>
		void Disconnect();
	}
}
