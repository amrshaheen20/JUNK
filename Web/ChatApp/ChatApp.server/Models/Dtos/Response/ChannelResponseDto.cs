using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{
    public class ChannelResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public eChannelType Type { get; set; }
        public string CreatedBy { get; set; }
        public string ServerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ChannelResponseDto(string id, string name, eChannelType type, DateTime createdAt, string memberId, string serverId)
        {
            Id = id;
            Name = name;
            Type = type;
            CreatedAt = createdAt;
            CreatedBy = memberId;
            ServerId = serverId;
        }

        public ChannelResponseDto(Channel channel)
        {
            Id = channel.Id;
            Name = channel.Name;
            Type = channel.Type;
            ServerId = channel.ServerId;
            CreatedAt = channel.CreatedAt;
            CreatedBy = channel.ProfileId;
        }
    }
}
