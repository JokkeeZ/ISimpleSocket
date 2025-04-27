using System.Net;
using System.Net.Sockets;
using ISimpleSocket.Events;

namespace ISimpleSocket;

public delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e);

public abstract class SimpleServer : ISimpleServer, IDisposable
{
    private Socket? _listener;
    private CancellationTokenSource? _cts;
    private readonly IPEndPoint _ipEndPoint;

    /// <summary>
    /// Gets a value indicating if server is listening for new connections.
    /// </summary>
    public bool Listening { get; private set; }

    public event AsyncEventHandler<ConnectionAcceptedEventArgs>? OnConnectionAccepted;
    public event AsyncEventHandler<ServerStartFailedEventArgs>? OnServerStartFailed;
    public event AsyncEventHandler<ConnectionRejectedEventArgs>? OnConnectionRejected;

    /// <summary>
    /// Unique <see cref="Guid"/> for current server instance.
    /// Used in <see cref="ServerMonitor"/> to identify each server.
    /// </summary>
    public Guid Id => Guid.NewGuid();

    /// <summary>
    /// Gets a value indicating active connections to the server.
    /// </summary>
    public int ConnectionCount => ServerMonitor.GetServerConnectionsCount(this);

    /// <summary>
    /// Gets a value of maximum connections accepted by current server instance. Default: 1000
    /// </summary>
    public int MaximumConnections { get; init; } = 1000;

    /// <summary>
    /// Gets a value of maximum length of pending connections queue.
    /// </summary>
    public int Backlog { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SimpleServer"/> with the <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="iPEndPoint">The <see cref="IPEndPoint"/> which represents local endpoint.</param>
    /// <param name="backlog">Maximum length of pending connections queue. Default value is 100.</param>
    protected SimpleServer(IPEndPoint iPEndPoint, int backlog = 100)
    {
        _ipEndPoint = iPEndPoint;
        Backlog = backlog;

        ServerMonitor.RegisterServer(this);
    }

    /// <summary>
    /// Starts listening for new connections asynchronously.
    /// </summary>
    public async Task StartAsync()
    {
        _cts = new();

        // Clear out old connections, if any.
        ServerMonitor.ClearServerConnections(this);

        _listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _listener.Bind(_ipEndPoint);
            _listener.Listen(Backlog);

            Listening = true;

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync(_cts.Token);

                    var monitorState = ServerMonitor.GetServerMonitorState(this);
                    if (monitorState == MonitorState.SlotsFull)
                    {
                        await RejectConnectionAsync(clientSocket);
                        continue;
                    }

                    var connectionId = ServerMonitor.GetServerFirstAvailableSlot(this);
                    ServerMonitor.AddConnectionToServer(this, connectionId);

                    if (OnConnectionAccepted != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            await OnConnectionAccepted(this, new(connectionId, clientSocket));
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (OnServerStartFailed != null)
                    {
                        await OnServerStartFailed(this, new(ex));
                    }

                    break;
                }
            }
        }
        catch (SocketException ex)
        {
            if (OnServerStartFailed != null)
            {
                await OnServerStartFailed(this, new(ex));
            }
        }
        finally
        {
            await StopInternalAsync();
        }
    }

    private async Task RejectConnectionAsync(Socket socket)
    {
        if (OnConnectionRejected != null)
        {
            await OnConnectionRejected(this, new(socket));
        }

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    private async Task StopInternalAsync()
    {
        if (_listener != null)
        {
            _listener.Close();
            _listener = null;
        }

        Listening = false;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the server, which makes the socket to stop accepting pending connections. 
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
    }

    /// <summary>
    /// Releases all resourced used by current instance of <see cref="SimpleServer"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases all resourced used by current instance of <see cref="SimpleServer"/>.
    /// </summary>
    /// <param name="disposing">
    /// If true, disposes all managed resourced used
    /// by current instance of <see cref="SimpleServer"/>.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _listener?.Dispose();

        ServerMonitor.UnregisterServer(this);
    }
}