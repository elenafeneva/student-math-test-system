namespace MathTaskValidator.Core.Models
{
    public class Teacher : BaseEntity
    {
        // External id from XML
        public string ExternalId { get; set; } = string.Empty;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public Teacher(string uniqueId)
        {
            UniqueId = uniqueId;
        }
    }
}
