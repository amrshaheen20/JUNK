using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class Conversation : BaseEntity
    {
        [ForeignKey(nameof(ProfileOne))]
        public string? ProfileOneId { get; set; }
        public Profile? ProfileOne { get; set; }

        [ForeignKey(nameof(ProfileTwo))]
        public string? ProfileTwoId { get; set; }
        public Profile? ProfileTwo { get; set; }

        [ForeignKey(nameof(Channel))]
        public string ChannelId { get; set; }
        public Channel Channel { get; set; }
    }

}
