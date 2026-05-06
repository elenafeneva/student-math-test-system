namespace MathTaskValidator.Core.Models
{
    public class Exam : BaseEntity
    {
        // External id from XML
        public string ExternalId { get; set; } = string.Empty;
        public Guid StudentId { get; set; }

        public virtual Student? Student { get; set; }
        public virtual ICollection<ExamTask> Tasks { get; set; } = new List<ExamTask>();

        public Exam(string uniqueId, Student student)
        {
            ExternalId = uniqueId;
            Student = student;
            StudentId = student.Id;
        }
    }
}
