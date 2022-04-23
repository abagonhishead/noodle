namespace Jossellware.Noodle.Web.Api.Mapping
{
	using System.Net;
	using Jossellware.Noodle.Shared.CQRS.Handlers.MagicPacket;
	using Jossellware.Noodle.Web.Api.Models;
	using Jossellware.Shared.Mapping.Maps;
	using Jossellware.Shared.Networking.Ip;

	public class MagicPacketRequestMapper : ClassMapBase<MagicPacketRequestModel, MagicPacketRequestHandler.Context>
	{
		protected override MagicPacketRequestHandler.Context BuildImplementation(MagicPacketRequestModel source, MagicPacketRequestHandler.Context? destination = null)
		{
			return new MagicPacketRequestHandler.Context(MacAddress.Parse(source.TargetMacAddress), IPAddress.Parse(source.BroadcastAddress));
		}
	}
}
