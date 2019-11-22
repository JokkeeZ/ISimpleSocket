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
	public abstract class SimpleServer : IDisposable
	{
		private readonly TcpListener _listener;

		private CancellationTokenSource _cts;
		private CancellationToken _token;

		private readonly ILog _log = LogManager.GetLogger(typeof(SimpleServer));

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
		/// Initializes an new instance of <see cref="SimpleServer"/> with the port.
		/// </summary>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		protected SimpleServer(int port)
		{
			_listener = new TcpListener(IPAddress.Any, port);
		}

		/// <summary>
		/// Initializes an new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>.
		/// </summary>
		/// <param name="endPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
		protected SimpleServer(IPEndPoint endPoint)
		{
			_listener = new TcpListener(endPoint);
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
			ConnectionMonitor.MaximumConnections = maxConnections;
		}

		/// <summary>
		/// Starts listening for new connections asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task StartAsync()
		{
			_cts = new CancellationTokenSource();
			_token = _cts.Token;

			if (!StartListener())
			{
				return;
			}

			var stopReason = 0;

			try
			{
				while (!_token.IsCancellationRequested)
				{
					await Task.Run(async () =>
					{
						if (ConnectionMonitor.State.Equals(MonitorState.SlotsAvailable))
						{
							var socket = await _listener.AcceptSocketAsync().ConfigureAwait(false);

							var connectionId = ConnectionMonitor.ConnectionsCount;
							OnConnectionReceived?.Invoke(this, new ConnectionReceivedEventArgs(connectionId, socket));

							_log.Debug($"New connection accepted with id: { connectionId }.");
						}
					})
					.ConfigureAwait(false);
				}
			}
			catch (SocketException socketEx)
			{
				_log.Error($"SocketException occurred, message: { socketEx.Message }", socketEx);
				stopReason = 1;
			}
			catch (ObjectDisposedException objectDisposedEx)
			{
				_log.Error($"ObjectDisposedException occurred, message: { objectDisposedEx.Message }", objectDisposedEx);
				stopReason = 2;
			}
			finally
			{
				_listener.Stop();
				Listening = false;

				if (stopReason != 0)
				{
					var x = stopReason == 1 ? "SocketException" : "ObjectDisposedException";
					_log.Debug($"Listener stopped. Reason: { x }.");
				}
			}
		}

		private bool StartListener()
		{
			try
			{
				// Clear out old connections, if any.
				ConnectionMonitor.Clear();

				_listener.Start(ConnectionMonitor.MaximumConnections);

				Listening = true;
				_log.Debug($"Listener started.");

				return true;
			}
			catch (SocketException ex)
			{
				OnServerStartFailed?.Invoke(this, new ServerStartFailedEventArgs(ex));

				_log.Error($"Failed to start listener, message: { ex.Message }", ex);
				return false;
			}
		}

		/// <summary>
		/// Cancels token source, which makes the asynchronous Task <see cref="StartAsync"/> stop listening for new connections.
		/// </summary>
		public void Stop() => _cts?.Cancel();

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
				_cts?.Cancel();
				_cts?.Dispose();

				_log.Debug($"Dispose({ disposing }) called, and object is disposed.");
			}
		}
	}
}
