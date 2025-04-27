namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> is sending data.
/// </summary>
/// <param name="data">Data to be sent.</param>
public sealed class ConnectionSendingDataEventArgs(byte[] data) : EventArgs
{
    /// <summary>
    /// Gets shallow copy of data that will be sent.
    /// </summary>
    /// <returns>Shallow copy of data that will be sent.</returns>
    public byte[] GetData()
    {
        return (byte[])data.Clone();
    }
}