namespace MathTaskValidator.Core.Models
{
    public class Exam : BaseEntity
    {
        public Guid StudentId { get; set; }

        public virtual Student? Student { get; set; }
        public virtual ICollection<ExamTask> Tasks { get; set; } = new List<ExamTask>();

        public Exam() { }

        public Exam(string uniqueId, Student student, string fileId)
        {
            UniqueId = uniqueId;
            ExternalId = fileId;
            Student = student;
            StudentId = student.Id;
        }
    }
}
