using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ISimpleSocket.Events;
using log4net;

namespace ISimpleSocket
{
	/// <summary>
	/// Provides a wrapper for <see cref="TcpListener"/> with asynchronous socket accepting.
	/// </summary>
	public abstract class SimpleServer : ISimpleServer, IDisposable
	{
		private readonly IPEndPoint ipEndPoint;
		private readonly ILog log = LogManager.GetLogger(typeof(SimpleServer));

		private ManualResetEvent newConnection;
		private CancellationTokenSource cts;

		/// <summary>
		/// Gets a value indicating if server is listening for new connections.
		/// </summary>
		public bool Listening { get; private set; }

		/// <summary>
		/// Occurs when new connection is accepted.
		/// </summary>
		public event EventHandler<ConnectionReceivedEventArgs> OnConnectionReceived;

		/// <summary>
		/// Occurs when server start has failed.
		/// </summary>
		public event EventHandler<ServerStartFailedEventArgs> OnServerStartFailed;

		/// <summary>
		/// Unique <see cref="Guid"/> for current server instance.
		/// Used in <see cref="ServerMonitor"/> to identify each servers.
		/// </summary>
		public Guid Id => Guid.NewGuid();

		/// <summary>
		/// Gets a value indicating active connections to the server.
		/// </summary>
		public int ConnectionsCount => ServerMonitor.GetServerConnectionsCount(this);

		/// <summary>
		/// Gets a value of maximum connections accepted by current server instance.
		/// </summary>
		public int MaximumConnections { get; } = 1000;

		/// <summary>
		/// Gets a value of maximum length of pending connections queue.
		/// </summary>
		public int Backlog { get; }

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the port.
		/// </summary>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		/// <param name="backlog">Maximum length of pending connections queue. Default value is 100.</param>
		protected SimpleServer(int port, int backlog = 100)
		{
			ipEndPoint = new(IPAddress.Any, port);
			ServerMonitor.RegisterServer(this);

			Backlog = backlog;
		}

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>.
		/// </summary>
		/// <param name="endPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
		/// <param name="backlog">Maximum length of pending connections queue. Default value is 100.</param>
		protected SimpleServer(IPEndPoint endPoint, int backlog = 100)
		{
			ipEndPoint = endPoint;
			ServerMonitor.RegisterServer(this);

			Backlog = backlog;
		}

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>
		/// and maximum amount of connections server will handle.
		/// </summary>
		/// <param name="endPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
		/// <param name="maxConnections">The amount of connections server will handle.</param>
		/// <param name="backlog">Maximum length of pending connections queue. Default value is 100.</param>
		protected SimpleServer(IPEndPoint endPoint, int maxConnections, int backlog = 100)
			: this(endPoint, backlog) => MaximumConnections = maxConnections;

		/// <summary>
		/// Starts listening for new connections asynchronously.
		/// </summary>
		public void StartListening()
		{
			cts = new();
			newConnection = new(false);

			// Clear out old connections, if any.
			ServerMonitor.ClearServerConnections(this);

			var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				listener.Bind(ipEndPoint);
				listener.Listen(Backlog);

				Listening = true;
				log.Info($"Server: { Id } started.");

				while (!cts.Token.IsCancellationRequested)
				{
					newConnection.Reset();

					listener.BeginAccept(new(AcceptConnectionCallback), listener);

					newConnection.WaitOne();
				}
			}
			catch (SocketException se)
			{
				OnServerStartFailed?.Invoke(this, new(se));
			}
			catch (ObjectDisposedException ode)
			{
				OnServerStartFailed?.Invoke(this, new(ode));
			}
			finally
			{
				listener.Shutdown(SocketShutdown.Both);
				listener.Close();

				Listening = false;
			}
		}

		private void AcceptConnectionCallback(IAsyncResult ar)
		{
			newConnection.Set();

			var listener = (Socket)ar.AsyncState;
			var clientSocket = listener.EndAccept(ar);

			if (ServerMonitor.GetServerMonitorState(this).Equals(MonitorState.SlotsFull))
			{
				RejectConnection(clientSocket);
				return;
			}

			var connectionId = ServerMonitor.GetServerFirstAvailableSlot(this);
			ServerMonitor.AddConnectionToServer(this, connectionId);

			OnConnectionReceived?.Invoke(this, new(connectionId, clientSocket));

			log.Info($"New connection accepted with id: { connectionId }.");
		}

		private void RejectConnection(Socket sck)
		{
			sck.Shutdown(SocketShutdown.Both);
			sck.Close();
			log.Info($"Server rejected connection. Reason: Server slots full.");
		}

		public void Stop() => cts?.Cancel();

		/// <summary>
		/// Releases all resourced used by current instance of <see cref="SimpleServer"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases all resourced used by current instance of <see cref="SimpleServer"/>.
		/// </summary>
		/// <param name="disposing">If true, disposes all managed resourced used by current instance of <see cref="SimpleServer"/>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				cts?.Cancel();
				cts?.Dispose();

				newConnection?.Dispose();

				ServerMonitor.UnRegisterServer(this);

				log.Debug($"Dispose({ disposing }) called, and object is disposed.");
			}
		}
	}
}
