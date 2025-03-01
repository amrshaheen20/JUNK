using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.DbSet
{
    public class BaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public BaseEntity()
        {
            DateTime _timestamp = DateTime.UtcNow;
            CreatedAt = _timestamp;
            UpdatedAt = _timestamp;
        }


    }
}
