namespace MathTaskValidator.Core.Models
{
    public class Student : BaseEntity
    {
        // External id from XML
        public string ExternalId { get; set; } = string.Empty;
        public Guid TeacherId { get; set; }

        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

        public Student(string uniqueId, Guid? teacherId)
        {
            UniqueId = uniqueId;
            TeacherId = teacherId ?? Guid.Empty;
        }
    }
}
