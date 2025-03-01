using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{
    public class ConversationResponseDto
    {
        public string Id { get; set; }
        public ProfileResponseDto TargetProfile { get; set; }
        public string ChannelId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ConversationResponseDto(string id, ProfileResponseDto targetProfile, DateTime createdAt)
        {
            Id = id;
            TargetProfile = targetProfile;
            CreatedAt = createdAt;
        }

        public ConversationResponseDto(Conversation conversation, Profile targetProfile)
        {
            Id = conversation.Id;
            this.TargetProfile = new ProfileResponseDto(targetProfile);
            CreatedAt = conversation.CreatedAt;
            ChannelId = conversation.ChannelId;
        }
    }
}
