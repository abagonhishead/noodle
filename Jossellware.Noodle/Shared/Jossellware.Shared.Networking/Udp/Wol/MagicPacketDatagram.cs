namespace Jossellware.Shared.Networking.Udp.Wol
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Text;
	using Jossellware.Shared.Networking.Ip;

	public readonly struct MagicPacketDatagram : IUdpDatagram
	{
		public static MagicPacketDatagram Create(IPAddress broadcastAddress, MacAddress hardwareAddress)
		{
			return new MagicPacketDatagram(broadcastAddress, hardwareAddress);
		}

		private const int DefaultWolPort = 9;

		private const int HardwareAddressRepetitions = 16;

		private static readonly byte[] SyncBytes = new byte[6]
		{
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff
		};

		private readonly byte[] data;

		public ReadOnlySpan<byte> Data
		{
			get => this.data;
		}

		public IPAddress Address { get; }

		public int Port { get; }

		private MagicPacketDatagram(IPAddress broadcastAddress, MacAddress hardwareAddress, int port)
		{
			if (port < 1 || port > 65535)
			{
				throw new ArgumentOutOfRangeException(nameof(port), "Must be between 1 and 65535");
			}

			this.Port = port;
			this.Address = broadcastAddress;

			this.data = new byte[MagicPacketDatagram.SyncBytes.Length + (hardwareAddress.Bytes.Length * MagicPacketDatagram.HardwareAddressRepetitions)];

			var index = 0;
			for (int i = 0; i < MagicPacketDatagram.SyncBytes.Length; i++)
			{
				this.data[index] = MagicPacketDatagram.SyncBytes[i];
				index++;
			}

			var addrRepetitions = 0;
			while (addrRepetitions < MagicPacketDatagram.HardwareAddressRepetitions)
			{
				for (int i = 0; i < hardwareAddress.Bytes.Length; i++)
				{
					this.data[index] = hardwareAddress.Bytes[i];
					index++;
				}

				addrRepetitions++;
			}
		}

		private MagicPacketDatagram(IPAddress broadcastAddress, MacAddress hardwareAddress)
			: this(broadcastAddress, hardwareAddress, DefaultWolPort)
		{
		}
	}
}
