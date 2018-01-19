// ---------------------------------------------------------------------------------
// <copyright file="SimpleConnection.cs" company="https://github.com/jokkeez/ISimpleSocket">
//   Copyright (c) 2018 JokkeeZ
// </copyright>
// <license>
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </license>
// ---------------------------------------------------------------------------------
using System;
using System.Net.Sockets;
using ISimpleSocket.Client.Events;

namespace ISimpleSocket.Client
{
	public abstract class SimpleConnection : IDisposable, ISimpleConnection
	{
		private readonly Socket _socket;
		private readonly byte[] _buffer;

		public int ConnectionId { get; private set; }

		public bool Disposed { get; private set; }

		public bool Connected => _socket != null && _socket.Connected;

		public event EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;

		public event EventHandler<ConnectionReceivedDataEventArgs> OnDataReceived;

		public event EventHandler<ConnectionSendingDataEventArgs> OnDataSend;

		public SimpleConnection(int id, Socket sck, int bufferSize = 1024)
		{
			ConnectionId = id;
			_socket = sck;

			_buffer = new byte[bufferSize];
		}

		public bool Start()
		{
			try
			{
				BeginReceive();
				return true;
			}
			catch
			{
				Disconnect();
				return false;
			}
		}

		private void DataReceived(IAsyncResult iAr)
		{
			var received = 0;

			try
			{
				received = _socket.EndReceive(iAr);
			}
			catch
			{
				Disconnect();
				return;
			}

			if (received <= 0)
			{
				Disconnect();
				return;
			}

			ProcessReceivedData(received);
			BeginReceive();
		}

		private void ProcessReceivedData(int received)
		{
			var data = new byte[received];
			Array.Copy(_buffer, 0, data, 0, received);

			OnDataReceived?.Invoke(this, new ConnectionReceivedDataEventArgs(data));
		}

		private void BeginReceive()
		{
			var error = SocketError.Success;
			_socket?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, out error, new AsyncCallback(DataReceived), null);

			if (error != SocketError.Success)
			{
				Disconnect();
			}
		}

		public void Disconnect()
		{
			try
			{
				_socket?.Shutdown(SocketShutdown.Both);
				_socket?.BeginDisconnect(false, _ => _socket?.EndDisconnect(_), null);
			}
			finally
			{
				if (!Disposed)
				{
					OnConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(this));
					Dispose();
				}
			}
		}

		public void SendData(byte[] data)
		{
			OnDataSend?.Invoke(this, new ConnectionSendingDataEventArgs(data));
			_socket?.BeginSend(data, 0, data.Length, 0, _ => _socket?.EndSend(_), null);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && (_socket != null))
			{
				_socket.Dispose();
				Disposed = true;
			}
		}
	}
}
