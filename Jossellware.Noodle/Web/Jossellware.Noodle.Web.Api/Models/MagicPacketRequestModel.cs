namespace Jossellware.Noodle.Web.Api.Models
{
	using System.Text.Json.Serialization;
	using Jossellware.Shared.Mapping.Models;

	public class MagicPacketRequestModel : IClassMapSource
	{
        /// <summary>
        /// A hardware MAC address to send the wake-on-LAN packet to.
        /// </summary>
        /// <example>c0:ff:33:c0:ff:33</example>
		[JsonPropertyName("targetMacAddress")]
		public string TargetMacAddress { get; set; }

        /// <summary>
        /// An IPv4 address to send the packet on. This will usually be the broadcast address for the subnet the machine sits under. For a /24 CIDR, set the last octet to 255.
        /// </summary>
        /// <example>192.168.200.255</example>
		[JsonPropertyName("broadcastAddress")]
		public string BroadcastAddress { get; set; }
	}
}
