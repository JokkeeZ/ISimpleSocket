using System;
using System.Net.Sockets;
using ISimpleSocket.Client.Events;
using log4net;

namespace ISimpleSocket.Client
{
	public abstract class SimpleConnection : IDisposable, ISimpleConnection
	{
		private readonly Socket _socket;
		private readonly byte[] _buffer;

		public int ConnectionId { get; private set; }
		public bool IsDisposed { get; private set; }

		public bool Connected => _socket != null && _socket.Connected;

		public event EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;
		public event EventHandler<ConnectionReceivedDataEventArgs> OnDataReceived;
		public event EventHandler<ConnectionSendingDataEventArgs> OnDataSend;
		public event EventHandler<ConnectionSocketErrorEventArgs> OnSocketError;

		private readonly ILog log = LogManager.GetLogger(typeof(SimpleConnection));

		protected SimpleConnection(int id, Socket sck, int bufferSize = 1024)
		{
			ConnectionId = id;

			_socket = sck ?? throw new ArgumentNullException(nameof(sck));
			_buffer = new byte[bufferSize];
		}

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
				_socket?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataReceived, null);
			}
			catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
			{
				log.Warn($"Connection with id: { ConnectionId } failed to begin receive incoming data. Exception message: { ex.Message }", ex);
				Disconnect();
			}
		}

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

		public void SendData(byte[] data)
		{
			OnDataSend?.Invoke(this, new ConnectionSendingDataEventArgs(data));
			_socket?.BeginSend(data, 0, data.Length, 0, _ => _socket?.EndSend(_), null);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

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
