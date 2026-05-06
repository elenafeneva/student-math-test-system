namespace MathTaskValidator.Core.Models
{
    public class ExamTask : BaseEntity
    {
        public string RawText { get; set; } = string.Empty;
        public Guid ExamId { get; set; }

        public virtual Exam? Exam { get; set; }
        
        public ExamTask() { }

        public ExamTask(string taskUniqueId, string rawText, string fileId)
        {
            UniqueId = taskUniqueId;
            RawText = rawText;
            ExternalId = fileId;
        }
    }
}
