// ---------------------------------------------------------------------------------
// <copyright file="ConnectionMonitor.cs" company="https://github.com/jokkeez/ISimpleSocket">
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
using System.Collections.Generic;
using System.Threading;

using ISimpleSocket.Client;

namespace ISimpleSocket
{
	internal sealed class ConnectionMonitor : IDisposable
	{
		private readonly Timer _timer;
		private readonly List<ISimpleConnection> _connections;

		private readonly int _maxConnections;

		public ConnectionMonitor(int maxConnectionsCount)
		{
			_maxConnections = maxConnectionsCount;

			_timer = new Timer(_ => MonitorDisposedConnection(), null, -1, -1);
			_connections = new List<ISimpleConnection>(_maxConnections);
		}

		public void AddConnection(ISimpleConnection connection)
		{
			_connections.Add(connection);

			if (_connections.Count == 1)
			{
				Start();
			}
		}

		private void Start()
		{
			_timer.Change(0, 500);
		}

		private void Stop()
		{
			_timer.Change(-1, -1);
		}

		private void MonitorDisposedConnection()
		{
			_connections.RemoveAll(x => x.Disposed);

			if (_connections.Count == 0)
			{
				Stop();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer?.Dispose();
			}
		}
	}
}
