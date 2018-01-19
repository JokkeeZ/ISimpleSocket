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
using System.Collections.Concurrent;

using ISimpleSocket.Client;
using log4net;

namespace ISimpleSocket
{
	internal static class ConnectionMonitor
	{
		private static readonly ConcurrentBag<ISimpleConnection> _slots = new ConcurrentBag<ISimpleConnection>();
		private static readonly ILog log = LogManager.GetLogger(typeof(ConnectionMonitor));

		public static int ConnectionsCount => _slots.Count;

		public static int MaximumConnections { get; internal set; } = 1000;

		public static MonitorState State
		{
			get
			{
				if (ConnectionsCount == MaximumConnections)
				{
					return MonitorState.SlotsFull;
				}

				return MonitorState.SlotsAvailable;
			}
		}

		public static void AddConnection(ISimpleConnection connection)
		{
			if (!_slots.TryTake(out connection))
			{
				_slots.Add(connection);
				log.Debug($"Added new connection. { _slots.Count } / { MaximumConnections } slots in-use.");
			}
		}

		public static void RemoveConnection(ISimpleConnection connection)
		{
			if (_slots.TryTake(out connection))
			{
				log.Debug($"Removed disposed connection. { _slots.Count } / { MaximumConnections } slots in-use.");
			}
		}
	}
}
