using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class Server : BaseEntity
    {
        public string Name { get; set; }

        [ForeignKey(nameof(Image))]
        public string? ImageId { get; set; }
        public Attachment? Image { get; set; }

        public string InviteCode { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey(nameof(Profile))]
        public string? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        public ICollection<Member> Members { get; set; } = new List<Member>();
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    }
}
