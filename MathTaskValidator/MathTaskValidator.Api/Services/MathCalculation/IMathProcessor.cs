
using MathTaskValidator.Core.Models;

namespace MathTaskValidator.Api.Services
{
    public interface IMathProcessor
    {
        List<ExamResult> ProcessExamResults(List<Exam> examResults);
    }
}
