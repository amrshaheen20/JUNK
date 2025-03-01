using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class Message : BaseEntity
    {
        public string? Content { get; set; }

        [ForeignKey(nameof(Profile))]
        public string? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        [ForeignKey(nameof(Channel))]
        public string ChannelId { get; set; }
        public Channel Channel { get; set; }

        public ICollection<Attachment>? Attachments { get; set; } = new List<Attachment>();
    }
}
