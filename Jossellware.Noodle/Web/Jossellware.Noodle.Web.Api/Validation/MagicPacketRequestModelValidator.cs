namespace Jossellware.Noodle.Web.Api.Validation
{
	using System.Net;
	using System.Net.Sockets;
	using FluentValidation;
	using Jossellware.Noodle.Web.Api.Models;
	using Jossellware.Shared.AspNetCore.Extensions.Validation.Fluent;
	using Jossellware.Shared.Networking.Ip;

	public class MagicPacketRequestModelValidator : AbstractValidator<MagicPacketRequestModel>
	{
		public MagicPacketRequestModelValidator()
		{
			this.RuleFor(x => x.TargetMacAddress)
				.NotNull()
				.NotEmpty()
				.WithMessage("{PropertyName} is required");
			this.RuleFor(x => x.TargetMacAddress)
				.Must(x => MacAddress.IsValid(x))
				.WithMessage("{PropertyName} must be a valid hardware MAC address");

            this.RuleFor(x => x.BroadcastAddress)
                .NotNull()
                .NotEmpty()
                .WithMessage("{PropertyName} is required");
            this.RuleFor(x => x.BroadcastAddress)
				.IpAddress()
				.WithMessage("{PropertyName} must be a valid IPv4 address");
		}
	}
}
