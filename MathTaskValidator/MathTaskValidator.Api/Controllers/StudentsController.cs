using MathTaskValidator.Api.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MathTaskValidator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{uniqueId}/results")]
        public async Task<QueryStudentResultsCommand.Response> GetStudentResultByUniqueId([FromRoute] string uniqueId)
        {
            var request = new QueryStudentResultsCommand.Request { StudentUniqueId = uniqueId };
            return await _mediator.Send(request);
        }
    }
}
