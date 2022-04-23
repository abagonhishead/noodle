namespace Jossellware.Noodle.Web.Api.Controllers.v1
{
    using System.Net.Mime;
    using Jossellware.Noodle.Shared.CQRS.Handlers.MagicPacket;
    using Jossellware.Noodle.Web.Api.Models;
    using Jossellware.Shared.AspNetCore.Mapping;
    using Jossellware.Shared.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("v1/[controller]", Name = Constants.RouteNames.MagicPacket.Prefix)]
    [Produces(MediaTypeNames.Application.Json)]
    public class MagicPacketController : ControllerBase
    {
        private readonly ILogger<MagicPacketController> logger;
        private readonly IMediator mediator;
        private readonly IClassMapProvider mapProvider;

        public MagicPacketController(ILogger<MagicPacketController> logger, IMediator mediator, IClassMapProvider mapProvider)
        {
            this.logger = logger;
            this.mediator = mediator;
            this.mapProvider = mapProvider;
        }

        /// <summary>
        /// Sends a wake-on-LAN 'magic' packet to a specified MAC address via a connected LAN broadcast address
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Nothing</returns>
        /// <response code="204">If the packet was sent successfully</response>
        /// <response code="400">If validation of the request failed</response>
        [HttpPost]
        [Route("send", Name = Constants.RouteNames.MagicPacket.SendPost)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendAsync(MagicPacketRequestModel model, CancellationToken cancellationToken)
        {
            using (this.logger.BeginMethodScope<MagicPacketController>(nameof(SendAsync)))
            {
                var context = this.mapProvider.Map<MagicPacketRequestModel, MagicPacketRequestHandler.Context>(model);
                await this.mediator.Send(context, cancellationToken);
                return this.NoContent();
            }
        }
    }
}
