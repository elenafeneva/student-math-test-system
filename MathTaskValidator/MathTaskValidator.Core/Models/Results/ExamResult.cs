namespace MathTaskValidator.Core.Models
{
    public class ExamResult
    {
        public ExamResult(string uniqueId, Guid studentId)
        {
            ExamUniqueId = uniqueId;
            StudentUniqueId = studentId;
        }

        public string ExamUniqueId { get; set; } = string.Empty;

        public Guid StudentUniqueId { get; set; }

        public List<TaskResult> TaskResults { get; set; } = new List<TaskResult>();

        public int CorrectCount => TaskResults.Count(t => t.IsCorrect);

        public int TotalCount => TaskResults.Count;

        public decimal Percentage => TotalCount == 0 ? 0 : (decimal)CorrectCount / TotalCount * 100;
    }
}
