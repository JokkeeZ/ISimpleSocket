using System.Net;
using System.Net.Sockets;
using System.Text;
using ISimpleSocket.Client;
using ISimpleSocket.Client.Events;
using ISimpleSocket.Events;

namespace ISimpleSocket.TestServer;

internal class ConnectionListener : SimpleServer
{
    public ConnectionListener(IPEndPoint ipEndPoint) : base(ipEndPoint)
    {
        MaximumConnections = 2;
        OnConnectionAccepted += ConnectionAccepted;
        OnConnectionRejected += ConnectionRejected;
    }

    private async Task ConnectionRejected(object sender, ConnectionRejectedEventArgs e)
    {

    }

    private async Task ConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
    {
        var connection = new Connection(this, e.Socket, e.ConnectionId);
        await connection.StartAsync();
    }
}

internal class Connection : SimpleConnection
{
    public Connection(ISimpleServer server, Socket socket, int id) : base(server, socket, id)
    {
        OnDataSend += DataSend;
        OnDataReceived += DataReceived;
        OnConnectionClosed += ConnectionClosed;
    }

    private async Task ConnectionClosed(object sender, ConnectionClosedEventArgs e)
    {
        // Do something when connection was closed.
    }

    private async Task DataReceived(object sender, ConnectionReceivedDataEventArgs e)
    {
        // Do something with received data.
    }

    private async Task DataSend(object sender, ConnectionSendingDataEventArgs e)
    {
        // Do something when data was sent.
    }
}

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "ISimpleSocket SERVER";

        using var listener = new ConnectionListener(new(IPAddress.Any, 2033));
        await listener.StartAsync();
    }
}
