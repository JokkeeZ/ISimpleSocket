using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
			OnServerStartFailed += ListenerStartFailed;
		}

		private void ListenerStartFailed(object sender, ServerStartFailedEventArgs e)
		{
			var exception = e.Exception.Message;
			Console.WriteLine($"Listener start failed. Exception: { exception }");
		}

		private void ConnectionReceived(object sender, ConnectionReceivedEventArgs e)
		{
			var connection = new Connection(e.ConnectionId, e.Socket);
			if (connection.Start())
			{
				AddConnection(connection);
				Console.WriteLine($"New connection added, with id: { connection.ConnectionId }");
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
			Console.WriteLine($"Connection with id: { ConnectionId } disconnected.");
		}

		private void DataReceived(object sender, ConnectionReceivedDataEventArgs e)
		{
			var data = Encoding.Default.GetString(e.ReceivedData);
			Console.WriteLine($"Server-side-client received: { data }");
		}

		private void DataSend(object sender, ConnectionSendingDataEventArgs e)
		{
			var data = Encoding.Default.GetString(e.Data);
			Console.WriteLine($"Server-side-client sent: { data }");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "ISimpleSocket SERVER";
			using (var listener = new ConnectionListener(port: 2033))
			{
				Task.Run(async () => await listener.StartAsync());
			}

			do
			{
				Thread.Sleep(1000);
			}
			while (Console.ReadLine() != "exit");
		}
	}
}
