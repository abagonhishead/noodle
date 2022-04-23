namespace Jossellware.Noodle.Shared.CQRS.Handlers.MagicPacket
{
	using System;
	using System.Net;
	using System.Threading;
	using System.Threading.Tasks;
	using Jossellware.Shared.CQRS;
	using Jossellware.Shared.Extensions;
	using Jossellware.Shared.Mapping.Models;
	using Jossellware.Shared.Networking.Ip;
	using Jossellware.Shared.Networking.Udp.Send;
	using Jossellware.Shared.Networking.Udp.Wol;
	using MediatR;
	using Microsoft.Extensions.Logging;

	public class MagicPacketRequestHandler : RequestHandlerBase<MagicPacketRequestHandler.Context>
    {
		private const int DefaultWakeOnLanPort = 9;

		private readonly IUdpTransmitter udpTransmitter;

		public MagicPacketRequestHandler(ILogger<MagicPacketRequestHandler> logger, IUdpTransmitter udpTransmitter) 
			: base(logger)
		{
			this.udpTransmitter = udpTransmitter;
		}

		protected override Task<Unit> HandleImplementationAsync(Context context, CancellationToken cancellationToken)
		{
			if (context.BroadcastAddress.AddressFamily != this.udpTransmitter.AddressFamily)
			{
				throw new InvalidOperationException($"Invalid IP address family for this UDP client ({context.BroadcastAddress.AddressFamily})");
			}

			var datagram = MagicPacketDatagram.Create(context.BroadcastAddress, context.HardwareAddress);
			this.udpTransmitter.SendAsync(datagram, cancellationToken);
			return Unit.Task;
		}

		public class Context : IRequest, IClassMapDestination
		{
			public MacAddress HardwareAddress { get; }

			public IPAddress BroadcastAddress { get; }

			public Context(MacAddress hardwareAddress, IPAddress broadcastAddress)
			{
				broadcastAddress.ThrowIfNull(nameof(broadcastAddress));

				this.HardwareAddress = hardwareAddress;
				this.BroadcastAddress = broadcastAddress;
			}
		}
    }
}
