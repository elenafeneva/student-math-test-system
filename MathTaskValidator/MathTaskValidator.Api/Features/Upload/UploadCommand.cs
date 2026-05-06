using FluentValidation;
using MathTaskValidator.Api.Services;
using MediatR;

namespace MathTaskValidator.Api.Features
{
    public class UploadCommand
    {
        public class Request : IRequest<Response>
        {
            public IFormFile File { get; set; } = null!;
        }

        public class Response
        {
            public string JobId { get; set; } = string.Empty;
            public string StatusUrl { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.File)
                    .NotNull()
                    .Must(file => file.Length > 0)
                    .WithMessage("File is required");
            }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly IUploadDataService _uploadDataService;

            public Handler(IUploadDataService uploadDataService)
            {
                _uploadDataService = uploadDataService;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var jobId = Guid.NewGuid().ToString();
                await _uploadDataService.UploadDataAsync(request.File);

                return new Response
                {
                    JobId = Guid.NewGuid().ToString(),
                    StatusUrl = "/api/exams/status/" + Guid.NewGuid().ToString()
                 };
            }
        }
    }
}
