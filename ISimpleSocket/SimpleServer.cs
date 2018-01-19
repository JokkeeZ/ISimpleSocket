// ---------------------------------------------------------------------------------
// <copyright file="SimpleServer.cs" company="https://github.com/jokkeez/ISimpleSocket">
//   Copyright (c) 2018 JokkeeZ
// </copyright>
// <license>
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </license>
// ---------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using ISimpleSocket.Client;
using ISimpleSocket.Events;
using log4net;

namespace ISimpleSocket
{
	public abstract class SimpleServer : IDisposable
	{
		private readonly TcpListener _listener;

		private CancellationTokenSource _cts;
		private CancellationToken _token;

		private bool _listening;

		public bool Listening => _listening;

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
							var socketTask = _listener.AcceptSocketAsync();
							var socket = await socketTask;

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
				_listening = false;

				log.Debug($"Listener stopped.");
			}
		}

		protected void AddConnection(ISimpleConnection connection)
		{
			ConnectionMonitor.AddConnection(connection);
		}

		private bool StartListener()
		{
			try
			{
				_listener.Start(ConnectionMonitor.MaximumConnections);

				_listening = true;
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
