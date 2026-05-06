using MathTaskValidator.Core.Models;
using org.mariuszgromada.math.mxparser;

namespace MathTaskValidator.Api.Services
{
    public class MathProcessor : IMathProcessor
    {
        public List<ExamResult> ProcessExamResultsAsync(List<Exam> examList)
        {
            var examResultsList = new List<ExamResult>();
            foreach (var exam in examList)
            {
                var examResult = new ExamResult(exam.UniqueId, exam.StudentId);

                foreach (var examTask in exam.Tasks)
                {
                    var task = examTask.RawText.Split('=')[0].Trim();
                    var studentAnswerStr = examTask.RawText.Split('=')[1].Trim();
                    if (double.TryParse(studentAnswerStr, out double studentAnswer))
                    {
                        var taskResult = IsResultCorrect(task, studentAnswer, examTask.UniqueId);
                        examResult.TaskResults.Add(taskResult);
                    }
                }
                examResultsList.Add(examResult);
            }

            return examResultsList;
        }

        private TaskResult IsResultCorrect(string expression, double studentAnswer, string uniqueId)
        {
            double actualResult = Calculate(expression);
            var isCorrect = Math.Abs(actualResult - studentAnswer) < 0.0001;
            return new TaskResult()
            {
                TaskUniqueId = uniqueId,
                ExpectedValue = (decimal)actualResult,
                StudentValue = (decimal)studentAnswer,
                IsCorrect = isCorrect,
                ErrorMessage = isCorrect ? "Correct result" : $"Expected {actualResult}, but got {studentAnswer}"
            };
        }

        private double Calculate(string expression)
        {
            Expression e = new Expression(expression);
            return e.calculate();
        }
    }
}
