using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ISimpleSocket.Client.Events;
using log4net;

namespace ISimpleSocket.Client
{
	/// <summary>
	/// Represents an connection for <see cref="SimpleServer"/> instance.
	/// </summary>
	public abstract class SimpleConnection : ISimpleConnection
	{
		private bool disposed;
		private readonly byte[] buffer;
		private readonly ILog log = LogManager.GetLogger(typeof(SimpleConnection));

		/// <summary>
		/// Gets current <see cref="System.Net.Sockets.Socket"/> instance.
		/// </summary>
		/// <returns>Returns current <see cref="System.Net.Sockets.Socket"/> instance.</returns>
		protected Socket Socket { get; }

		/// <summary>
		/// Gets a value that indicates, if connection is connected to the server.
		/// </summary>
		public bool Connected => Socket != null && Socket.Connected;

		/// <summary>
		/// Unique connection id.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// Gets a server where connection belongs.
		/// </summary>
		public ISimpleServer Server { get; }

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
		/// <param name="server"><see cref="ISimpleServer"/> instance where connection was created.</param>
		/// <param name="id">Connection id.</param>
		/// <param name="socket">Connection socket.</param>
		/// <param name="bufferSize">Maximum amount of bytes buffer can have. Default: 1024</param>
		protected SimpleConnection(ISimpleServer server, Socket socket, int id, int bufferSize = 1024)
		{
			Id = id;
			Server = server;

			Socket = socket ?? throw new ArgumentNullException(nameof(socket));
			buffer = new byte[bufferSize];
		}

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket and buffer size.
		/// </summary>
		/// <param name="server"><see cref="ISimpleServer"/> instance where connection was created.</param>
		/// <param name="socket">Connection socket.</param>
		/// <param name="bufferSize">Maximum amount of bytes buffer can have. Default: 1024</param>
		protected SimpleConnection(ISimpleServer server, Socket socket, int bufferSize = 1024)
			: this(server, socket, 0, bufferSize) { }

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket, and default buffer size: 1024.
		/// </summary>
		/// <param name="server"><see cref="ISimpleServer"/> instance where connection was created.</param>
		/// <param name="socket">Connection socket.</param>
		protected SimpleConnection(ISimpleServer server, Socket socket)
			: this(server, socket, 1024) { }

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket, and default buffer size: 1024.
		/// <para><see cref="ISimpleServer"/> instance will not be set.</para>
		/// </summary>
		/// <param name="socket">Connection socket.</param>
		protected SimpleConnection(Socket socket)
			: this(null, socket, 1024) { }

		/// <summary>
		/// Initializes an new instance of the <see cref="SimpleConnection"/> with socket, and given buffer size.
		/// <para><see cref="ISimpleServer"/> instance will not be set.</para>
		/// </summary>
		/// <param name="socket">Connection socket.</param>
		/// <param name="bufferSize">Maximum amount of bytes buffer can have. Default: 1024</param>
		protected SimpleConnection(Socket socket, int bufferSize = 1024)
			: this(null, socket, bufferSize) { }

		/// <summary>
		/// Establishes a connection to a server.
		/// </summary>
		/// <param name="endpoint">An EndPoint that represents the server address.</param>
		/// <returns>Returns <see cref="SocketError.Success"/>, if connection was created; otherwise <see cref="SocketError"/>.</returns>
		protected async Task<SocketError> ConnectAsync(IPEndPoint endpoint)
		{
			try
			{
				await Socket.ConnectAsync(endpoint).ConfigureAwait(false);
				return SocketError.Success;
			}
			catch (SocketException ex)
			{
				return ex.SocketErrorCode;
			}
		}

		/// <summary>
		/// Starts receiving data from server, if connected.
		/// </summary>
		/// <returns>Returns true, if data was received successfully; otherwise false.</returns>
		public bool Start()
		{
			try
			{
				if (!Connected)
				{
					return false;
				}

				BeginReceive();

				log.Debug($"Connection started with id: { Id }");

				if (Server != null)
				{
					ServerMonitor.AddConnectionToServer(Server, Id);
				}

				return true;
			}
			catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
			{
				log.Error($"Connection with id: { Id } failed to start receiving data. Message: { ex.Message }");

				Disconnect();
				return false;
			}
		}

		private void BeginReceive()
		{
			try
			{
				var error = SocketError.Success;

				var test = Socket?.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out error, DataReceived, null);

				if (error != SocketError.Success)
				{
					OnSocketError?.Invoke(this, new(error));
				}
			}
			catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
			{
				log.Debug($"Connection with id: { Id } failed to begin receive incoming data. Reason: { ex.Message }, Connection will disconnect.");
				Disconnect();
			}
		}

		private void DataReceived(IAsyncResult iAr)
		{
			var received = Socket.EndReceive(iAr, out var error);

			if (error != SocketError.Success)
			{
				OnSocketError?.Invoke(this, new(error));

				Disconnect();
				return;
			}

			if (received == 0 && !disposed)
			{
				Disconnect();
				return;
			}

			ProcessReceivedData(received);
			BeginReceive();
		}

		private void ProcessReceivedData(int received)
		{
			var data = new byte[received];
			Array.Copy(buffer, 0, data, 0, received);

			OnDataReceived?.Invoke(this, new(data));
		}

		/// <summary>
		/// Disconnects from the server.
		/// </summary>
		public void Disconnect()
		{
			try
			{
				if (Connected && !disposed)
				{
					Socket?.Shutdown(SocketShutdown.Both);
					Socket?.BeginDisconnect(false, _ => Socket?.EndDisconnect(_), null);

					log.Debug($"Connection with id: { Id } disconnected.");
				}
			}
			catch (ObjectDisposedException)
			{
				log.Debug($"Connection with id: { Id } trying to disconnect disposed connection.");
			}
			catch (SocketException e)
			{
				log.Debug($"Connection with id: { Id } got an SocketException. Message: {e.Message}");
			}
			finally
			{
				if (!disposed)
				{
					log.Debug($"Connection with id: { Id } firing OnConnectionClosed event.");

					OnConnectionClosed?.Invoke(this, new(this));
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
			if (data is null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			try
			{
				var error = SocketError.Success;

				Socket?.BeginSend(data, 0, data.Length, 0, out error, _ => Socket?.EndSend(_), null);
				OnDataSend?.Invoke(this, new(data));

				if (error != SocketError.Success)
				{
					OnSocketError?.Invoke(this, new(error));
				}
			}
			catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
			{
				log.Debug($"Connection with id: { Id } failed to send data. Reason: { ex.Message }, Connection will disconnect.");
				Disconnect();
			}
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
			if (!disposed && disposing)
			{
				Socket?.Dispose();
				disposed = true;

				if (Server != null)
				{
					ServerMonitor.RemoveConnectionFromServer(Server, Id);
				}

				log.Debug($"Connection with id: { Id } called Dispose({ disposing }) and disposed.");
			}
		}
	}
}
