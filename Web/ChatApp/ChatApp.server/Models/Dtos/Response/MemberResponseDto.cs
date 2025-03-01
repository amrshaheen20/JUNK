using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{
    public class MemberResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string? ImageId { get; set; }
        public eMemberRole Role { get; set; }

        public MemberResponseDto(string id, string userName, string? imageId, eMemberRole role)
        {
            Id = id;
            UserName = userName;
            ImageId = imageId;
            Role = role;
        }

        public MemberResponseDto(Member member)
        {
            Id = member.ProfileId;
            UserName = member.Profile.DisplayName;
            ImageId = member.Profile.ImageId;
            Role = member.Role;
        }




    }
}
