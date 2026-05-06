namespace MathTaskValidator.Core.Models
{
    public class ExamTask : BaseEntity
    {
        // Task id from XML
        public string ExternalId { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public Guid ExamId { get; set; }

        public virtual Exam? Exam { get; set; }
    }
}
