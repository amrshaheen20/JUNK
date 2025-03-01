using ChatApi.server.Models.DbSet;
using System.Text.Json.Serialization;

namespace ChatApi.server.Models.Dtos.Response
{
    public class ProfileResponseDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string? ImageId { get; set; }
        public string Email { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinTime { get; set; }

        public ProfileResponseDto() { }
        
        public ProfileResponseDto(Profile profile)
        {
            Id = profile.Id;
            DisplayName = profile.DisplayName;
            UserName = profile.UserName;
            ImageId = profile.ImageId;
            Email = profile.Email;
            Bio = profile.Bio;
            JoinTime = profile.CreatedAt;
        }


    }


    public class ProfileWithMoreDataResponseDto: ProfileResponseDto
    {
        public List<MutualServerDto> MutualServers { get; set; } = new List<MutualServerDto>();
        public FriendshipStatus? FriendshipStatus { get; set; }

        public ProfileWithMoreDataResponseDto() { }

        public ProfileWithMoreDataResponseDto(Profile profile, List<MutualServerDto> mutualServers, FriendshipStatus? friendshipStatus) : base(profile)
        {
            MutualServers = mutualServers;
            FriendshipStatus = friendshipStatus;
        }
    }

}
