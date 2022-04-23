namespace Jossellware.Shared.Networking.Udp
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Text;

	public interface IUdpDatagram
	{
		public ReadOnlySpan<byte> Data { get; }

		public IPAddress Address { get; }

		public int Port { get; }
	}
}
