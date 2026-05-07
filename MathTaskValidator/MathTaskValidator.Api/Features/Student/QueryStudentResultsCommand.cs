using FluentValidation;
using MathTaskValidator.Api.Services;
using MathTaskValidator.Core.Models;
using MediatR;

namespace MathTaskValidator.Api.Features
{
    public class QueryStudentResultsCommand
    {
        public class Request : IRequest<Response>
        {
            public string StudentUniqueId { get; set; } = string.Empty;
        }

        public class Response
        {
            public List<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.StudentUniqueId)
                    .NotEmpty()
                    .WithMessage("StudentUniqueId is required");
            }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly IStudentService _studentService;
            private readonly IMathProcessor _mathProcessor;
            public Handler(IStudentService studentService, IMathProcessor mathProcessor)
            {
                _studentService = studentService;
                _mathProcessor = mathProcessor;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                //Check if the user exist in the system
                var student = await _studentService.GetStudentByUniqueIdAsync(request.StudentUniqueId);
                if (student is null)
                    return new Response();

                var studentExams = student.Exams.ToList();
                if (studentExams.Count() <= 0)
                    return new Response();

                var studentExamResults = _mathProcessor.ProcessExamResults(studentExams);
                return new Response { ExamResults = studentExamResults };
            }
        }
    }
}
