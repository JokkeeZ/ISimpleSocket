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
