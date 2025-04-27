namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has received data.
/// </summary>
/// <param name="data">Data that was received</param>
public sealed class ConnectionReceivedDataEventArgs(byte[] data) : EventArgs
{
    /// <summary>
    /// Gets a shallow copy of data that was received.
    /// </summary>
    /// <returns>Shallow copy of data that was received.</returns>
    public byte[] GetReceivedData()
    {
        return (byte[])data.Clone();
    }
}