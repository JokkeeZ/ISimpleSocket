using System.Collections.Generic;
using log4net;

namespace ISimpleSocket
{
	internal static class ConnectionMonitor
	{
		static readonly IList<int> _slots = new List<int>();
		static readonly ILog _log = LogManager.GetLogger(typeof(ConnectionMonitor));

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

		public static void AddConnection(int connectionId)
		{
			if (!_slots.Contains(connectionId))
			{
				_slots.Add(connectionId);
				_log.Debug($"Added new connection. { _slots.Count } / { MaximumConnections } slots in-use.");
			}
		}

		public static void RemoveConnection(int connectionId)
		{
			if (_slots.Contains(connectionId))
			{
				_slots.Remove(connectionId);
				_log.Debug($"Removed disposed connection. { _slots.Count } / { MaximumConnections } slots in-use.");
			}
		}

		public static void Clear() => _slots.Clear();
	}
}
