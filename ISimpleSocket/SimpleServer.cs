using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISimpleSocket.Events;
using log4net;

namespace ISimpleSocket
{
	public abstract class SimpleServer : IDisposable
	{
		private readonly TcpListener _listener;

		private CancellationTokenSource _cts;
		private CancellationToken _token;

		public bool Listening { get; private set; }

		public event EventHandler<ConnectionReceivedEventArgs> OnConnectionReceived;
		public event EventHandler<ServerStartFailedEventArgs> OnServerStartFailed;

		private readonly ILog _log = LogManager.GetLogger(typeof(SimpleServer));

		protected SimpleServer(int port)
		{
			_listener = new TcpListener(IPAddress.Any, port);
		}

		protected SimpleServer(IPEndPoint endPoint)
		{
			_listener = new TcpListener(endPoint);
		}

		protected SimpleServer(IPEndPoint endPoint, int maxConnections)
			: this(endPoint)
		{
			ConnectionMonitor.MaximumConnections = maxConnections;
		}

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
				_log.Error($"SocketException occurred, message: {socketEx.Message}", socketEx);
				stopReason = 1;
			}
			catch (ObjectDisposedException objectDisposedEx)
			{
				_log.Error($"ObjectDisposedException occurred, message: {objectDisposedEx.Message}", objectDisposedEx);
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

		public void Stop() => _cts?.Cancel();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

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
