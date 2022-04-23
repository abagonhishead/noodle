namespace Jossellware.Noodle.Shared.Domain.MagicPacket
{
    using System;
    using System.Collections.Generic;
	using System.Net;
	using System.Text;
	using Jossellware.Shared.Mapping.Models;
	using Jossellware.Shared.Networking.Ip;

	public class WakeOnLanModel : IClassMapSource, IClassMapDestination
    {
		public MacAddress HardwareAddress { get; set; }

		public IPAddress BroadcastAddress { get; set; }
    }
}
