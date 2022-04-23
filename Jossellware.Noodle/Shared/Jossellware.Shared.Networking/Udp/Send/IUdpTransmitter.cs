namespace Jossellware.Shared.Networking.Udp.Send
{
	using System.Net.Sockets;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IUdpTransmitter
	{
		AddressFamily AddressFamily { get; }
		bool BroadcastEnabled { get; }

		void Send(IUdpDatagram datagram);
		Task SendAsync(IUdpDatagram datagram, CancellationToken cancellationToken);
	}
}