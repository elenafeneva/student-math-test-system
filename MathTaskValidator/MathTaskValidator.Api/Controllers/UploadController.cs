using MathTaskValidator.Api.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MathTaskValidator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UploadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<UploadCommand.Response> Upload([FromForm] UploadCommand.Request request)
            => await _mediator.Send(request);
    }
}
