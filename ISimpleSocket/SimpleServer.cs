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

namespace ISimpleSocket
{
	public abstract class SimpleServer : IDisposable
	{
		private readonly TcpListener _listener;
		private readonly ConnectionMonitor _connectionMonitor;

		private CancellationTokenSource _cts;
		private CancellationToken _token;

		private bool _listening;
		private int _maxConnections = 1000;

		public bool Listening => _listening;

		public event EventHandler<ConnectionReceivedEventArgs> OnConnectionReceived;
		public event EventHandler<ServerStartFailedEventArgs> OnListenerStartFailed;

		public SimpleServer(int port)
		{
			_listener = new TcpListener(IPAddress.Any, port);
			_connectionMonitor = new ConnectionMonitor(_maxConnections);
		}

		public SimpleServer(IPEndPoint endPoint)
		{
			_listener = new TcpListener(endPoint);
			_connectionMonitor = new ConnectionMonitor(_maxConnections);
		}

		public SimpleServer(IPEndPoint endPoint, int maxConnectionsCount)
			: this(endPoint)
		{
			_maxConnections = maxConnectionsCount;
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
						var socketTask = _listener.AcceptSocketAsync();
						var socket = await socketTask;

						OnConnectionReceived?.Invoke(this, new ConnectionReceivedEventArgs(socket));
					});
				}
			}
			finally
			{
				_listener.Stop();
				_listening = false;
			}
		}

		protected void AddConnection(ISimpleConnection connection)
		{
			_connectionMonitor.AddConnection(connection);
		}

		private bool StartListener()
		{
			try
			{
				_listener.Start(_maxConnections);

				_listening = true;
				return true;
			}
			catch (SocketException ex)
			{
				OnListenerStartFailed?.Invoke(this, new ServerStartFailedEventArgs(ex));
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

				_connectionMonitor?.Dispose();
			}
		}
	}
}
