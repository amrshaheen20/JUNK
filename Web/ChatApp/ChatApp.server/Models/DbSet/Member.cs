
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApi.server.Models.DbSet
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum eMemberRole : int
    {
        ADMIN,
        MODERATOR,
        MEMBER
    }

    public class Member : BaseEntity
    {
        public eMemberRole Role { get; set; } = eMemberRole.MEMBER;

        [ForeignKey(nameof(Profile))]
        public string ProfileId { get; set; }

        public Profile Profile { get; set; }

        [ForeignKey(nameof(Server))]
        public string ServerId { get; set; }
        public Server Server { get; set; }
    }
}
