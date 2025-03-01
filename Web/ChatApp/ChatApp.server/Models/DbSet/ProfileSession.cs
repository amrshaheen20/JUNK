using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class ProfileSession : BaseEntity
    {

        [ForeignKey(nameof(Profile))]
        public string ProfileId { get; set; }
        public Profile Profile { get; set; }
        public string TokenId { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string? UserAgent { get; set; }
    }
}
