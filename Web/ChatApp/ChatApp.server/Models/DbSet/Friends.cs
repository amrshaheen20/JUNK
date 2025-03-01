using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApi.server.Models.DbSet
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Blocked
    }
    public class Friendship : BaseEntity
    {
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        public Profile? User { get; set; }

        [ForeignKey(nameof(Friend))]
        public string? FriendId { get; set; }
        public Profile? Friend { get; set; }

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending; 

    }
}
