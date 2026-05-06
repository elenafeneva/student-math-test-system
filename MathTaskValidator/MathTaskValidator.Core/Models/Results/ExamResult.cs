namespace MathTaskValidator.Core.Models
{
    public class ExamResult
    {
        public string ExamId { get; set; } = string.Empty;

        public string StudentId { get; set; } = string.Empty;

        public List<TaskResult> TaskResults { get; set; } = new List<TaskResult>();

        public int CorrectCount => TaskResults.Count(t => t.IsCorrect);

        public int TotalCount => TaskResults.Count;

        public decimal Percentage => TotalCount == 0 ? 0 : (decimal)CorrectCount / TotalCount * 100;
    }
}
