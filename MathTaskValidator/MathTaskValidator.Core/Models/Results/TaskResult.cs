namespace MathTaskValidator.Core.Models
{
    public class TaskResult
    {
        public string TaskUniqueId { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public decimal StudentValue { get; set; }
        public bool IsCorrect { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
