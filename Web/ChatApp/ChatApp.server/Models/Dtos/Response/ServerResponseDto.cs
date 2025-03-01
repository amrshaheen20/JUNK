using ChatApi.server.Models.DbSet;


namespace ChatApi.server.Models.Dtos.Response
{
    public class ServerResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? ImageId { get; set; }
        public string InviteCode { get; set; }
        public int MembersCount { get; set; }
        public int ChannelsCount { get; set; }
        public DateTime? MemberSince { get; set; }

        public ServerResponseDto(string id, string name, string imageId, string inviteCode, int membersCount, int channelsCount, DateTime memberSince)
        {
            Id = id;
            Name = name;
            ImageId = imageId;
            InviteCode = inviteCode;
            MembersCount = membersCount;
            ChannelsCount = channelsCount;
            MemberSince = memberSince;
        }

        public ServerResponseDto(Server server, string profileId)
        {
            Id = server.Id;
            Name = server.Name;
            ImageId = server.ImageId;
            InviteCode = server.InviteCode;
            MembersCount = server.Members.Count();
            ChannelsCount = server.Channels.Count();
            MemberSince = server.Members.FirstOrDefault(m => m.ProfileId == profileId)?.CreatedAt;
        }
    }
}
