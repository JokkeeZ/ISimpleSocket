using System;

namespace ISimpleSocket
{
	/// <summary>
	/// Represents an server which accepts <see cref="Client.ISimpleConnection"/> connection requests.
	/// </summary>
	public interface ISimpleServer
	{
		/// <summary>
		/// Unique <see cref="Guid"/> for current server instance.
		/// Used in <see cref="ServerMonitor"/> to identify each servers.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Gets a value of maximum connections accepted by current server instance.
		/// </summary>
		int MaximumConnections { get; }

		/// <summary>
		/// Maximum length of pending connections queue.
		/// </summary>
		int Backlog { get; }
	}
}

namespace System.Runtime.CompilerServices
{
	public class IsExternalInit { }
}
