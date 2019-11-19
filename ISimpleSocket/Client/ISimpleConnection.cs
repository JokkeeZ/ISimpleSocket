namespace ISimpleSocket.Client
{
	public interface ISimpleConnection
	{
		int ConnectionId { get; }
		bool IsDisposed { get; }
		bool Connected { get; }

		void SendData(byte[] data);
		void Disconnect();
	}
}
