using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class Profile : IdentityUser
    {
        public string DisplayName { get; set; }

        [ForeignKey(nameof(Image))]
        public string? ImageId { get; set; }
        public Attachment? Image { get; set; }

        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastActiveTime { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<Server> Servers { get; set; } = new List<Server>();
        public ICollection<Member> Members { get; set; } = new List<Member>();
        public ICollection<Channel> ChannelsCreatedByUser { get; set; } = new List<Channel>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Conversation> ConversationsInitiated { get; set; } = new List<Conversation>();
        public ICollection<Conversation> ConversationsReceived { get; set; } = new List<Conversation>();
        public ICollection<ProfileSession> ProfileSessions { get; set; } = new List<ProfileSession>();
        public ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();


        public Profile()
        {
            DateTime _timestamp = DateTime.UtcNow;
            CreatedAt = _timestamp;
            UpdatedAt = _timestamp;
            LastActiveTime = _timestamp;
        }
    }
}
