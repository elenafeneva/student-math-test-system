namespace MathTaskValidator.Core.Models
{
    public class Student : BaseEntity
    {
        public Guid TeacherId { get; set; }

        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

        public Student() { }

        public Student(string uniqueId, Guid? teacherId, string fileId)
        {
            UniqueId = uniqueId;
            TeacherId = teacherId ?? Guid.Empty;
            ExternalId = fileId;
        }
    }
}
