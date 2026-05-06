using MathTaskValidator.Api.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MathTaskValidator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeachersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{uniqueId}/students/results")]
        public async Task<QueryStudentsResultsCommand.Response> GetStudentsResultsByTeacherId([FromRoute] string uniqueId)
        {
            var request = new QueryStudentsResultsCommand.Request { TeacherUniqueId = uniqueId };
            return await _mediator.Send(request);
        }
    }
}
