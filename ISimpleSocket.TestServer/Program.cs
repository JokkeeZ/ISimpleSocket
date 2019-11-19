using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using ISimpleSocket.Client;
using ISimpleSocket.Client.Events;
using ISimpleSocket.Events;

namespace ISimpleSocket.TestServer
{
	class ConnectionListener : SimpleServer
	{
		public ConnectionListener(int port) : base(port)
		{
			OnConnectionReceived += ConnectionReceived;
		}

		private void ConnectionReceived(object sender, ConnectionReceivedEventArgs e)
		{
			var connection = new Connection(e.ConnectionId, e.Socket);
			if (connection.Start())
			{
				// Do something with connection.
			}
		}
	}

	class Connection : SimpleConnection
	{
		public Connection(int id, Socket socket) : base(id, socket)
		{
			OnDataSend += DataSend;
			OnDataReceived += DataReceived;
			OnConnectionClosed += ConnectionClosed;
		}

		private void ConnectionClosed(object sender, ConnectionClosedEventArgs e)
		{
			// Do something when connection was closed.
		}

		private void DataReceived(object sender, ConnectionReceivedDataEventArgs e)
		{
			// Do something with received data.
		}

		private void DataSend(object sender, ConnectionSendingDataEventArgs e)
		{
			// Do something when data was sent.
		}
	}

	class Program
	{
		static async Task Main(string[] args)
		{
			Console.Title = "ISimpleSocket SERVER";

			using var listener = new ConnectionListener(port: 2033);
			await Task.Run(async () => await listener.StartAsync());
		}
	}
}
