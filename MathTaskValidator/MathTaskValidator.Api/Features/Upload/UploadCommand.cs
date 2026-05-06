using FluentValidation;
using MathTaskValidator.Api.Services;
using MediatR;

namespace MathTaskValidator.Api.Features
{
    public class UploadCommand
    {
        public class Request : IRequest<Response>
        {
            public string TeacherUniqueId { get; set; }
            public IFormFile File { get; set; }
        }

        public class Response
        {
            public bool Success { get; set; }
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
                var success = await _uploadDataService.UploadDataAsync(request.File, request.TeacherUniqueId);
                return new Response { Success = success };
            }
        }
    }
}
