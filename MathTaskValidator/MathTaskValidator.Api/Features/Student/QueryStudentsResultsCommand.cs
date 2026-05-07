using FluentValidation;
using MathTaskValidator.Api.Services;
using MathTaskValidator.Core.Models;
using MediatR;

namespace MathTaskValidator.Api.Features
{
    public class QueryStudentsResultsCommand
    {
        public class Request : IRequest<Response>
        {
            public string TeacherUniqueId { get; set; } = string.Empty;
        }

        public class Response
        {
            public List<StudentResult> StudentsResults { get; set; } = new();

            public class StudentResult
            {
                public string StudentUniqueId { get; set; }
                public List<ExamResult> ExamResults { get; set; }
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.TeacherUniqueId)
                    .NotEmpty()
                    .WithMessage("TeacherUniqueId is required");
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
                var students = await _studentService.GetStudentsByTeacherUniqueIdAsync(request.TeacherUniqueId);
                if(!students.Any())
                    return new Response();

                var results = new List<Response.StudentResult>();
                foreach (var student in students)
                {
                    var studentExams = student.Exams.ToList();
                    var studentExamResults = _mathProcessor.ProcessExamResults(studentExams);

                    results.Add(new Response.StudentResult
                    {
                        StudentUniqueId = student.UniqueId,
                        ExamResults = studentExamResults
                    });
                }

                return new Response { StudentsResults = results };
            }
        }
    }
}
