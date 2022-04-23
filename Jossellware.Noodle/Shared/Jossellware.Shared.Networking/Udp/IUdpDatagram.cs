namespace Jossellware.Shared.Networking.Udp
{
    using System;
    using System.Net;

    public interface IUdpDatagram
	{
		public ReadOnlySpan<byte> Data { get; }

		public IPAddress Address { get; }

		public int Port { get; }
	}
}
