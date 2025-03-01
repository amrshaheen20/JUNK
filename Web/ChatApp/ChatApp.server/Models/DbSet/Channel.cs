using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApi.server.Models.DbSet
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum eChannelType
    {
        TEXT,
        CONVERSATION,
        VIDEO,
        AUDIO,
    }

    public class Channel : BaseEntity
    {
        public string Name { get; set; }
        public eChannelType Type { get; set; } = eChannelType.TEXT;

        public string? Category { get; set; }

        [ForeignKey(nameof(Profile))]
        public string? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        [ForeignKey(nameof(Server))]
        public string? ServerId { get; set; }
        public Server? Server { get; set; }

        public Conversation? Conversation { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();

    }

}
