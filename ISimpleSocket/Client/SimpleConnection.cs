using System;
using System.Net.Sockets;
using ISimpleSocket.Client.Events;
using log4net;

namespace ISimpleSocket.Client
{
	/// <summary>
	/// Represents an connection for <see cref="SimpleServer"/> instance.
	/// </summary>
	public abstract class SimpleConnection : ISimpleConnection
	{
		private readonly Socket _socket;
		private readonly byte[] _buffer;

		private readonly ILog log = LogManager.GetLogger(typeof(SimpleConnection));

		/// <summary>
		/// Unique connection id.
		/// </summary>
		public int ConnectionId { get; private set; }

		/// <summary>
		/// Gets a value that indicates, if connection is disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Gets a value that indicates, if connection is connected to the server.
		/// </summary>
		public bool Connected => _socket != null && _socket.Connected;

		/// <summary>
		/// On connection closed event handler.
		/// </summary>
		public event EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;

		/// <summary>
		/// On data received event handler.
		/// </summary>
		public event EventHandler<ConnectionReceivedDataEventArgs> OnDataReceived;

		/// <summary>
		/// On data send event handler.
		/// </summary>
		public event EventHandler<ConnectionSendingDataEventArgs> OnDataSend;

		/// <summary>
		/// On socket error event handler.
		/// </summary>
		public event EventHandler<ConnectionSocketErrorEventArgs> OnSocketError;

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with id, socket and buffer size.
		/// </summary>
		/// <param name="id">Connection id.</param>
		/// <param name="socket">Connection socket.</param>
		/// <param name="bufferSize">Maximum amount of bytes buffer can have.</param>
		protected SimpleConnection(int id, Socket socket, int bufferSize = 1024)
		{
			ConnectionId = id;

			_socket = socket ?? throw new ArgumentNullException(nameof(socket));
			_buffer = new byte[bufferSize];
		}

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket and buffer size.
		/// </summary>
		/// <param name="socket">Connection socket.</param>
		/// <param name="bufferSize">Maximum amount of bytes buffer can have.</param>
		protected SimpleConnection(Socket socket, int bufferSize = 1024)
			: this(0, socket, bufferSize) { }

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket.
		/// </summary>
		/// <param name="socket">Connection socket.</param>
		protected SimpleConnection(Socket socket)
			: this(socket, 1024) { }

		/// <summary>
		/// Starts connection to the server.
		/// </summary>
		/// <returns>Returns true if connection started successfully; otherwise false.</returns>
		public bool Start()
		{
			try
			{
				BeginReceive();

				log.Debug($"Connection started with id: { ConnectionId }");

				ConnectionMonitor.AddConnection(ConnectionId);
				return true;
			}
			catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
			{
				log.Error($"Failed to start connection with id: { ConnectionId }, exception message: { ex.Message }", ex);

				Disconnect();
				return false;
			}
		}

		private void DataReceived(IAsyncResult iAr)
		{
			int received;

			try
			{
				received = _socket.EndReceive(iAr, out var error);
				if (error != SocketError.Success)
				{
					OnSocketError?.Invoke(this, new ConnectionSocketErrorEventArgs(error));
					Disconnect();
					return;
				}
			}
			catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
			{
				log.Error(ex.Message, ex);

				Disconnect();
				return;
			}

			if (received != 0)
			{
				ProcessReceivedData(received);
			}

			BeginReceive();
		}

		private void ProcessReceivedData(int received)
		{
			var data = new byte[received];
			Array.Copy(_buffer, 0, data, 0, received);

			OnDataReceived?.Invoke(this, new ConnectionReceivedDataEventArgs(data));
		}

		private void BeginReceive()
		{
			try
			{
				var error = SocketError.Success;

				_socket?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, out error, DataReceived, null);
				if (error != SocketError.Success)
				{
					OnSocketError?.Invoke(this, new ConnectionSocketErrorEventArgs(error));
				}
			}
			catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
			{
				log.Warn($"Connection with id: { ConnectionId } failed to begin receive incoming data. Exception message: { ex.Message }", ex);
				Disconnect();
			}
		}

		/// <summary>
		/// Disconnects from the server.
		/// </summary>
		public void Disconnect()
		{
			try
			{
				_socket?.Shutdown(SocketShutdown.Both);
				_socket?.BeginDisconnect(false, _ => _socket?.EndDisconnect(_), null);

				log.Debug($"Connection with id: { ConnectionId } disconnected.");
			}
			finally
			{
				if (!IsDisposed)
				{
					log.Debug($"Connection with id: { ConnectionId } firing OnConnectionClosed event.");

					OnConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(this));
					Dispose();
				}
			}
		}

		/// <summary>
		/// Sends data to the server.
		/// </summary>
		/// <param name="data">Data to be sent.</param>
		public void SendData(byte[] data)
		{
			OnDataSend?.Invoke(this, new ConnectionSendingDataEventArgs(data));
			_socket?.BeginSend(data, 0, data.Length, 0, _ => _socket?.EndSend(_), null);
		}

		/// <summary>
		/// Releases all resources used by the current instance of the <see cref="SimpleConnection"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases all resourced used by current instance of <see cref="SimpleConnection"/>.
		/// </summary>
		/// <param name="disposing">If true, disposes all managed resourced used by current instance of <see cref="SimpleConnection"/>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && (_socket != null))
			{
				_socket.Dispose();
				IsDisposed = true;

				ConnectionMonitor.RemoveConnection(ConnectionId);

				log.Debug($"Connection with id: { ConnectionId } called Dispose({ disposing }) and disposed.");
			}
		}
	}
}
