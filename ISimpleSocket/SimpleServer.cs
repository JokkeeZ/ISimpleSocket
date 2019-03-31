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

		private readonly ILog log = LogManager.GetLogger(typeof(SimpleServer));

		public SimpleServer(int port)
		{
			_listener = new TcpListener(IPAddress.Any, port);
		}

		public SimpleServer(IPEndPoint endPoint)
		{
			_listener = new TcpListener(endPoint);
		}

		public SimpleServer(IPEndPoint endPoint, int maxConnections)
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

			try
			{
				while (!_token.IsCancellationRequested)
				{
					await Task.Run(async () =>
					{
						if (ConnectionMonitor.State.Equals(MonitorState.SlotsAvailable))
						{
							var socket = await _listener.AcceptSocketAsync();

							var connectionId = ConnectionMonitor.ConnectionsCount;
							OnConnectionReceived?.Invoke(this, new ConnectionReceivedEventArgs(connectionId, socket));

							log.Debug($"New connection accepted with id: { connectionId }.");
						}
					});
				}
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
			}
			finally
			{
				_listener.Stop();
				Listening = false;

				log.Debug($"Listener stopped.");
			}
		}

		private bool StartListener()
		{
			try
			{
				_listener.Start(ConnectionMonitor.MaximumConnections);

				Listening = true;
				log.Debug($"Listener started.");
				return true;
			}
			catch (SocketException ex)
			{
				OnServerStartFailed?.Invoke(this, new ServerStartFailedEventArgs(ex));

				log.Error($"Failed to start listener, message: { ex.Message }", ex);
				return false;
			}
		}

		public void Stop() => _cts?.Cancel();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cts?.Cancel();
				_cts?.Dispose();

				log.Debug($"Dispose({ disposing }) called, and object is disposed.");
			}
		}
	}
}
