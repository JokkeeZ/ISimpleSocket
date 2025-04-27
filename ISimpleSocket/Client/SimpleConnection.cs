using System.Net.Sockets;
using ISimpleSocket.Client.Events;

namespace ISimpleSocket.Client;

/// <summary>
/// Represents a connection for <see cref="SimpleServer"/> instance.
/// </summary>
public abstract class SimpleConnection : ISimpleConnection, IDisposable
{
    private const int DefaultBufferSize = 1024;

    private bool _disposed;
    private readonly byte[] _buffer;

    /// <summary>
    /// Gets current <see cref="System.Net.Sockets.Socket"/> instance.
    /// </summary>
    /// <returns>Returns current <see cref="System.Net.Sockets.Socket"/> instance.</returns>
    protected Socket Socket { get; }

    /// <summary>
    /// Gets a value that indicates, if connection is connected to the server.
    /// </summary>
    public bool Connected => Socket.Connected;

    /// <summary>
    /// Unique connection id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets a server where connection belongs.
    /// </summary>
    public ISimpleServer Server { get; }

    /// <summary>
    /// On connection closed event handler.
    /// </summary>
    public event AsyncEventHandler<ConnectionClosedEventArgs>? OnConnectionClosed;

    /// <summary>
    /// On data received event handler.
    /// </summary>
    public event AsyncEventHandler<ConnectionReceivedDataEventArgs>? OnDataReceived;

    /// <summary>
    /// On data send event handler.
    /// </summary>
    public event AsyncEventHandler<ConnectionSendingDataEventArgs>? OnDataSend;

    /// <summary>
    /// On socket error event handler.
    /// </summary>
    public event AsyncEventHandler<ConnectionSocketErrorEventArgs>? OnSocketError;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleConnection"/> with id, socket and buffer size.
    /// </summary>
    /// <param name="server"><see cref="ISimpleServer"/> instance where connection was created.</param>
    /// <param name="id">Connection id.</param>
    /// <param name="socket">Connection socket.</param>
    /// <param name="bufferSize">Maximum amount of bytes buffer can have. Default: 1024</param>
    protected SimpleConnection(ISimpleServer server, Socket socket, int id, int bufferSize = DefaultBufferSize)
    {
        Id = id;
        Server = server;

        Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _buffer = new byte[bufferSize];
    }

    /// <summary>
    /// Starts receiving data from server, if connected.
    /// </summary>
    /// <returns>Returns true, if data was received successfully; otherwise false.</returns>
    public async Task StartAsync()
    {
        if (!Connected)
        {
            return;
        }

        ServerMonitor.AddConnectionToServer(Server, Id);

        try
        {
            await ReceiveLoopAsync();
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            await DisconnectAsync();
        }
    }

    private async Task ReceiveLoopAsync()
    {
        try
        {
            while (Connected && !_disposed)
            {
                var result = await Socket.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);

                if (result == 0)
                {
                    await DisconnectAsync();
                    break;
                }

                if (OnDataReceived != null)
                {
                    await OnDataReceived(this, new(_buffer[..result]));
                }
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// Disconnects from the server.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!Connected || _disposed)
        {
            return;
        }

        try
        {
            Socket.Shutdown(SocketShutdown.Both);
            await Socket.DisconnectAsync(false);
        }
        catch (Exception)
        {
            /* ignore errors during shutdown */
        }
        finally
        {
            if (!_disposed)
            {
                if (OnConnectionClosed != null)
                {
                    await OnConnectionClosed(this, new(this));
                }

                Dispose();
            }
        }
    }

    /// <summary>
    /// Sends data to the server.
    /// </summary>
    /// <param name="data">Data to be sent.</param>
    public async Task SendDataAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            var sent = await Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            if (OnDataSend != null)
            {
                await OnDataSend(this, new(data));
            }

            if (sent != data.Length)
            {
                if (OnSocketError != null)
                {
                    await OnSocketError(this, new(SocketError.SocketError));
                }
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="SimpleConnection"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases all resourced used by current instance of <see cref="SimpleConnection"/>.
    /// </summary>
    /// <param name="disposing">If true, disposes all managed resourced used by current instance of <see cref="SimpleConnection"/>.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        Socket.Dispose();
        _disposed = true;

        ServerMonitor.RemoveConnectionFromServer(Server, Id);
    }
}