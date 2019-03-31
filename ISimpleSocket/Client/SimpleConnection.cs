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

		public bool Disposed { get; private set; }

		public bool Connected => _socket != null && _socket.Connected;

		public event EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;
		public event EventHandler<ConnectionReceivedDataEventArgs> OnDataReceived;
		public event EventHandler<ConnectionSendingDataEventArgs> OnDataSend;

		private readonly ILog log = LogManager.GetLogger(typeof(SimpleConnection));

		public SimpleConnection(int id, Socket sck, int bufferSize = 1024)
		{
			ConnectionId = id;
			_socket = sck;

			_buffer = new byte[bufferSize];
		}

		public bool Start()
		{
			try
			{
				BeginReceive();

				log.Debug($"Connection started with id: { ConnectionId }");
				return true;
			}
			catch (Exception ex)
			{
				log.Error($"Failed to start connection with id: { ConnectionId }, exception message: { ex.Message }", ex);

				Disconnect();
				return false;
			}
		}

		private void DataReceived(IAsyncResult iAr)
		{
			var received = 0;

			try
			{
				received = _socket.EndReceive(iAr);
			}
			catch (ObjectDisposedException)
			{
				log.Warn($"Connection with id: { ConnectionId } failed to read incoming data. (Socket was disposed)");
				return;
			}
			catch (Exception ex)
			{
				log.Warn($"Connection with id: { ConnectionId } failed to read incoming data. Exception message: { ex.Message }", ex);

				Disconnect();
				return;
			}

			if (received <= 0)
			{
				log.Warn($"Connection with id: { ConnectionId } received zero bytes.");

				Disconnect();
				return;
			}

			ProcessReceivedData(received);

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
				_socket?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(DataReceived), null);
			}
			catch (Exception ex)
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
				if (!Disposed)
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

		private void Dispose(bool disposing)
		{
			if (disposing && (_socket != null))
			{
				_socket.Dispose();
				Disposed = true;

				ConnectionMonitor.RemoveConnection(this);

				log.Debug($"Connection with id: { ConnectionId } called  Dispose({ disposing }) and disposed.");
			}
		}
	}
}
