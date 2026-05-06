namespace MathTaskValidator.Core.Models
{
    public class Teacher : BaseEntity
    {
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public Teacher() { }

        public Teacher(string uniqueId)
        {
            UniqueId = uniqueId;
        }
    }
}
