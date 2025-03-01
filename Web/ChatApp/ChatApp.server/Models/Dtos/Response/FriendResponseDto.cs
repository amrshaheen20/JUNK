using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{

    public class FriendResponseDto
    {
        public ProfileResponseDto Profile { get; set; }

        public ProfileResponseDto Friend { get; set; }

        public FriendshipStatus Status { get; set; }

        public FriendResponseDto() { }
        public FriendResponseDto(ProfileResponseDto profile, ProfileResponseDto friend, FriendshipStatus status)
        {
            Profile = profile;
            Friend = friend;
            Status = status;
        }

    }








}
