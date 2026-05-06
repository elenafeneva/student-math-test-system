
using MathTaskValidator.Core.Models;

namespace MathTaskValidator.Api.Services
{
    public interface IMathProcessor
    {
        List<ExamResult> ProcessExamResultsAsync(List<Exam> examResults);
    }
}
