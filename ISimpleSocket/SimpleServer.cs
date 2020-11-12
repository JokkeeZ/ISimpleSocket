using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISimpleSocket.Events;
using log4net;

namespace ISimpleSocket
{
	/// <summary>
	/// Provides a wrapper for <see cref="TcpListener"/> with asynchronous socket accepting.
	/// </summary>
	public abstract class SimpleServer : ISimpleServer, IDisposable
	{
		private readonly TcpListener listener;
		private readonly ILog log = LogManager.GetLogger(typeof(SimpleServer));

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
		/// Initializes an new instance of <see cref="SimpleServer"/> with the port.
		/// </summary>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		protected SimpleServer(int port)
		{
			listener = new TcpListener(IPAddress.Any, port);
			ServerMonitor.RegisterServer(this);
		}

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>.
		/// </summary>
		/// <param name="endPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
		protected SimpleServer(IPEndPoint endPoint)
		{
			listener = new TcpListener(endPoint);
			ServerMonitor.RegisterServer(this);
		}

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>
		/// and maximum amount of connections server will handle.
		/// </summary>
		/// <param name="endPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
		/// <param name="maxConnections">The amount of connections server will handle.</param>
		protected SimpleServer(IPEndPoint endPoint, int maxConnections)
			: this(endPoint)
		{
			MaximumConnections = maxConnections;
		}

		/// <summary>
		/// Starts listening for new connections asynchronously.
		/// </summary>
		public async Task StartAsync()
		{
			cts = new CancellationTokenSource();

			if (!StartListener())
			{
				return;
			}

			var stopReason = 0;

			try
			{
				while (!cts.Token.IsCancellationRequested)
				{
					await Task.Run(async () =>
					{
						if (ServerMonitor.GetServerMonitorState(this).Equals(MonitorState.SlotsAvailable))
						{
							var socket = await listener.AcceptSocketAsync().ConfigureAwait(false);

							var connectionId = ServerMonitor.GetServerFirstAvailableSlot(this);
							OnConnectionReceived?.Invoke(this, new ConnectionReceivedEventArgs(connectionId, socket));

							log.Info($"New connection accepted with id: { connectionId }.");
						}
					})
					.ConfigureAwait(false);
				}
			}
			catch (SocketException socketEx)
			{
				log.Fatal($"SocketException occurred, message: { socketEx.Message }");
				stopReason = 1;
			}
			catch (ObjectDisposedException objectDisposedEx)
			{
				log.Fatal($"ObjectDisposedException occurred, message: { objectDisposedEx.Message }");
				stopReason = 2;
			}
			finally
			{
				listener.Stop();
				Listening = false;

				if (stopReason != 0)
				{
					var x = stopReason == 1 ? "SocketException" : "ObjectDisposedException";
					log.Debug($"Listener stopped. Reason: { x }.");
				}
			}
		}

		private bool StartListener()
		{
			try
			{
				// Clear out old connections, if any.
				ServerMonitor.ClearServerConnections(this);

				listener.Start(MaximumConnections);

				Listening = true;
				log.Info($"Server: { Id } started.");

				return true;
			}
			catch (SocketException ex)
			{
				OnServerStartFailed?.Invoke(this, new ServerStartFailedEventArgs(ex.SocketErrorCode));

				log.Fatal($"Failed to start listener, message: { ex.Message }");
				return false;
			}
		}

		/// <summary>
		/// Cancels token source, which makes the asynchronous Task <see cref="StartAsync"/> to stop listening for new connections.
		/// </summary>
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

				ServerMonitor.UnRegisterServer(this);

				log.Debug($"Dispose({ disposing }) called, and object is disposed.");
			}
		}
	}
}
