namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> has received data.
/// </summary>
/// <remarks>
/// Initializes an new instance of the <see cref="ConnectionReceivedDataEventArgs"/>, with data that was received.
/// </remarks>
/// <param name="data">Data that was received</param>
public sealed class ConnectionReceivedDataEventArgs(byte[] data) : EventArgs
{
	private readonly byte[] data = data;

	/// <summary>
	/// Gets a shallow copy of data that was received.
	/// </summary>
	/// <returns>Shallow copy of data that was received.</returns>
	public byte[] GetReceivedData() => (byte[])data.Clone();
}
