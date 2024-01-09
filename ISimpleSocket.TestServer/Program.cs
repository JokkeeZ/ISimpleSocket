using System.Net.Sockets;
using ISimpleSocket.Client;
using ISimpleSocket.Client.Events;
using ISimpleSocket.Events;

namespace ISimpleSocket.TestServer;

class ConnectionListener : SimpleServer
{
	public ConnectionListener(int port) : base(port)
	{
		OnConnectionAccepted += ConnectionAccepted;
	}

	private void ConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
	{
		var connection = new Connection(this, e.Socket, e.ConnectionId);
		if (connection.Start())
		{
			// Do something with connection.
		}
	}
}

class Connection : SimpleConnection
{
	public Connection(ISimpleServer server, Socket socket, int id) : base(server, socket, id)
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
	static void Main()
	{
		Console.Title = "ISimpleSocket SERVER";

		using var listener = new ConnectionListener(port: 2033);
		listener.StartListening();
	}
}
