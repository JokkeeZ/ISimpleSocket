using System.Collections.Generic;
using log4net;

namespace ISimpleSocket
{
	internal static class ServerMonitor
	{
		static readonly IDictionary<ISimpleServer, List<int>> servers = new Dictionary<ISimpleServer, List<int>>();
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
			var connectionsCount = GetServerConnectionsCount(server);
			return connectionsCount == server.MaximumConnections ? MonitorState.SlotsFull : MonitorState.SlotsAvailable;
		}

		public static int GetServerFirstAvailableSlot(ISimpleServer server)
		{
			var connectionsCount = GetServerConnectionsCount(server);

			if (!servers[server].Contains(connectionsCount - 1) && (connectionsCount - 1 >= 0))
			{
				return connectionsCount - 1;
			}

			return connectionsCount;
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
				log.Info($"Added new connection to server: { server.Id }. { servers[server].Count } / { server.MaximumConnections } slots in-use.");
			}
		}

		public static void RemoveConnectionFromServer(ISimpleServer server, int connectionId)
		{
			if (!IsServerRegistered(server))
			{
				return;
			}

			servers[server].Remove(connectionId);
			log.Info($"Removed connection from server: { server.Id }. { servers[server].Count } / { server.MaximumConnections } slots in-use.");
		}

		public static void ClearServerConnections(ISimpleServer server)
		{
			if (!IsServerRegistered(server))
			{
				return;
			}

			servers[server].Clear();
		}

		public static void RegisterServer<T>(T server) where T : ISimpleServer
		{
			if (!IsServerRegistered(server))
			{
				servers.Add(server, new List<int>(server.MaximumConnections));
			}
		}

		public static void UnRegisterServer<T>(T server) where T : ISimpleServer
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
