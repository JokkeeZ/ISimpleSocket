namespace ISimpleSocket
{
	internal static class ServerMonitor
	{
		static readonly Dictionary<ISimpleServer, List<int>> servers = new();

		/// <summary>
		/// Gets server connections count, if server exists in <see cref="ServerMonitor"/>.
		/// </summary>
		/// <param name="server">Server to get connections count from.</param>
		/// <returns>Returns server connections count, if server exists; otherwise -1.</returns>
		public static int GetServerConnectionsCount(ISimpleServer server)
		{
			if (!IsServerRegistered(server))
			{
				return -1;
			}

			return servers[server].Count;
		}

		/// <summary>
		/// Gets server <see cref="MonitorState"/>. (If server accepts new connections.)
		/// </summary>
		/// <param name="server">Server to check for current state.</param>
		/// <returns>Returns <see cref="MonitorState.SlotsAvailable"/>, if server accepts new connections;
		/// otherwise <see cref="MonitorState.SlotsFull"/>.</returns>
		public static MonitorState GetServerMonitorState(ISimpleServer server)
		{
			var count = GetServerConnectionsCount(server);
			return count == server.MaximumConnections ? MonitorState.SlotsFull : MonitorState.SlotsAvailable;
		}

		/// <summary>
		/// Gets first available slot in server connections list.
		/// </summary>
		/// <param name="server">Server to get available slot.</param>
		/// <returns>Returns first available slot in the server.</returns>
		public static int GetServerFirstAvailableSlot(ISimpleServer server)
		{
			var count = GetServerConnectionsCount(server);

			if (!servers[server].Contains(count - 1) && (count - 1 >= 0))
			{
				return count - 1;
			}

			return count;
		}

		/// <summary>
		/// Adds connection to server by given connection id.
		/// </summary>
		/// <param name="server">Server where connection will be added.</param>
		/// <param name="connectionId">Connection id.</param>
		public static void AddConnectionToServer(ISimpleServer server, int connectionId)
		{
			if (!IsServerRegistered(server))
			{
				return;
			}

			if (!servers[server].Contains(connectionId))
			{
				servers[server].Add(connectionId);
			}
		}

		/// <summary>
		/// Removes connection by id, from given server.
		/// </summary>
		/// <param name="server">Server where connection will be removed.</param>
		/// <param name="connectionId">Connection id.</param>
		/// <returns>Returns <see langword="true"/>, if server was found and connection was removed successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public static bool RemoveConnectionFromServer(ISimpleServer server, int connectionId)
		{
			if (!IsServerRegistered(server))
			{
				return false;
			}

			return servers[server].Remove(connectionId);
		}

		/// <summary>
		/// Clears server connections List, if server exists in <see cref="ServerMonitor"/>.
		/// </summary>
		/// <param name="server">Server which connections List is cleared.</param>
		public static void ClearServerConnections(ISimpleServer server)
		{
			if (IsServerRegistered(server))
			{
				servers[server].Clear();
			}
		}

		/// <summary>
		/// Adds server to <see cref="ServerMonitor"/>, if it doesn't already exist.
		/// </summary>
		/// <param name="server">Server to add in to <see cref="ServerMonitor"/>.</param>
		public static void RegisterServer(ISimpleServer server)
		{
			if (!IsServerRegistered(server))
			{
				servers.Add(server, new(server.MaximumConnections));
			}
		}

		/// <summary>
		/// Removes server from <see cref="ServerMonitor"/>, if it exists.
		/// </summary>
		/// <param name="server">Server to remove from <see cref="ServerMonitor"/>.</param>
		public static void UnregisterServer(ISimpleServer server)
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
