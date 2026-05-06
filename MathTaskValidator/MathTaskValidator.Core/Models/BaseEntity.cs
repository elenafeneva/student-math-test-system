namespace MathTaskValidator.Core.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UniqueId { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTimeOffset LastModifiedOnUtc { get; set; }
    }
}
