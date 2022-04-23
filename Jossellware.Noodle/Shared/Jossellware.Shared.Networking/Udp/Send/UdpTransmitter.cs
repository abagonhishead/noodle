namespace Jossellware.Shared.Networking.Udp.Send
{
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading;
	using System.Threading.Tasks;

	public class UdpTransmitter : IUdpTransmitter, IDisposable
	{
		private bool disposed;
		private UdpClient client;

		public AddressFamily AddressFamily { get; }

		public bool BroadcastEnabled { get; }

		public UdpTransmitter()
			: this(AddressFamily.InterNetwork)
		{
		}

		public UdpTransmitter(bool broadcastEnabled)
			: this(AddressFamily.InterNetwork, broadcastEnabled)
		{
		}

		public UdpTransmitter(AddressFamily addressFamily, bool broadcastEnabled = false)
		{
			this.AddressFamily = addressFamily;
			this.BroadcastEnabled = broadcastEnabled;
		}

		public void Send(IUdpDatagram datagram)
		{
			this.EnsureClientReady();

			var endpoint = new IPEndPoint(datagram.Address, datagram.Port);
            this.client.Send(datagram.Data.ToArray(), datagram.Data.Length, endpoint);
		}

		public Task SendAsync(IUdpDatagram datagram, CancellationToken cancellationToken)
		{
			this.EnsureClientReady();

			cancellationToken.ThrowIfCancellationRequested();
			var endpoint = new IPEndPoint(datagram.Address, datagram.Port);
			return this.client.SendAsync(datagram.Data.ToArray(), datagram.Data.Length, endpoint);
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				if (this.client != null)
				{
					try
					{
						this.client.Close();
					}
					finally
					{
						this.client.Dispose();
					}
				}

				this.disposed = true;
			}
		}

		private void EnsureClientReady()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if (this.client == null)
			{
				this.client = new UdpClient(this.AddressFamily);
				this.client.EnableBroadcast = this.BroadcastEnabled;
			}
		}
	}
}
