namespace ISimpleSocket.Client.Events;

/// <summary>
/// Represents event arguments for event, which occurs when <see cref="SimpleConnection"/> is sending data.
/// </summary>
/// <remarks>
/// Initializes an new instance of the <see cref="ConnectionSendingDataEventArgs"/> with data to be sent.
/// </remarks>
/// <param name="data">Data to be sent.</param>
public sealed class ConnectionSendingDataEventArgs(byte[] data) : EventArgs
{
	private readonly byte[] data = data;

	/// <summary>
	/// Gets shallow copy of data that will be sent.
	/// </summary>
	/// <returns>Shallow copy of data that will be sent.</returns>
	public byte[] GetData() => (byte[])data.Clone();
}
