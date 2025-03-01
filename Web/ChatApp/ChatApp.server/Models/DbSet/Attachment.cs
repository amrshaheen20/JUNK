using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.server.Models.DbSet
{
    public class Attachment : BaseEntity
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long Length { get; set; }
        public string Md5Hash { get; set; }
        [ForeignKey(nameof(Profile))]
        public string? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        [ForeignKey(nameof(Message))]
        public string? MessageId { get; set; }
        public Message? Message { get; set; }

    }
}
