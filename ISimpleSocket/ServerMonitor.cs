using System.Collections.Generic;
using log4net;

namespace ISimpleSocket
{
	internal static class ServerMonitor
	{
		static readonly Dictionary<ISimpleServer, List<int>> servers = new();
		static readonly ILog log = LogManager.GetLogger(typeof(ServerMonitor));

		public static int GetServerConnectionsCount(ISimpleServer server)
		{
			if (!IsServerRegistered(server))
			{
				return -1;
			}

			return servers[server].Count;
		}

		public static MonitorState GetServerMonitorState(ISimpleServer server)
		{
			var count = GetServerConnectionsCount(server);
			return count == server.MaximumConnections ? MonitorState.SlotsFull : MonitorState.SlotsAvailable;
		}

		public static int GetServerFirstAvailableSlot(ISimpleServer server)
		{
			var count = GetServerConnectionsCount(server);

			if (!servers[server].Contains(count - 1) && (count - 1 >= 0))
			{
				return count - 1;
			}

			return count;
		}

		public static void AddConnectionToServer(ISimpleServer server, int connectionId)
		{
			if (!IsServerRegistered(server))
			{
				return;
			}

			if (!servers[server].Contains(connectionId))
			{
				servers[server].Add(connectionId);
				log.Debug($"Added new connection to server: { server.Id }. { servers[server].Count } / { server.MaximumConnections } slots in-use.");
			}
		}

		public static void RemoveConnectionFromServer(ISimpleServer server, int connectionId)
		{
			if (!IsServerRegistered(server))
			{
				return;
			}

			servers[server].Remove(connectionId);
			log.Debug($"Removed connection from server: { server.Id }. { servers[server].Count } / { server.MaximumConnections } slots in-use.");
		}

		public static void ClearServerConnections(ISimpleServer server)
		{
			if (IsServerRegistered(server))
			{
				servers[server].Clear();
			}
		}

		public static void RegisterServer<T>(T server) where T : ISimpleServer
		{
			if (!IsServerRegistered(server))
			{
				servers.Add(server, new(server.MaximumConnections));
			}
		}

		public static void UnregisterServer<T>(T server) where T : ISimpleServer
		{
			if (IsServerRegistered(server))
			{
				ClearServerConnections(server);
				servers.Remove(server);
			}
		}

		static bool IsServerRegistered(ISimpleServer server) => servers.ContainsKey(server);
	}
}
