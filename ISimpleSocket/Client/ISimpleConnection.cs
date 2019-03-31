namespace ISimpleSocket.Client
{
	public interface ISimpleConnection
	{
		int ConnectionId { get; }
		bool Disposed { get; }
		bool Connected { get; }

		void SendData(byte[] data);
		void Disconnect();
	}
}
