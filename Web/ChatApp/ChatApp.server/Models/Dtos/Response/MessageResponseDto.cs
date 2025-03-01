using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{
    public class MessageResponseDto
    {
        public string Id { get; set; } = null!;
        public string? Content { get; set; } = null!;
        public string ChannelId { get; set; } = null!;
        public IEnumerable<AttachmentResponseDto> Attachments { get; set; } = new List<AttachmentResponseDto>();
        public AuthorResponseDto Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; } = false;


        public MessageResponseDto(Message message, bool isMember = true)
        {
            ChannelId = message.ChannelId;
            Id = message.Id;
            Content = message.Content;
            Attachments = message.Attachments
                        .Select(a => new AttachmentResponseDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            ContentType = a.ContentType
                        }).OrderBy(x => x.ContentType);
            Author = new AuthorResponseDto
            {
                Id = message?.ProfileId,
                ImageId = message?.Profile?.ImageId,
                UserName = message?.Profile?.DisplayName,
                IsMember = isMember
            };
            CreatedAt = message.CreatedAt;

            IsEdited = message.CreatedAt != message.UpdatedAt;
        }
    }


    public class AttachmentResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }


    public class AuthorResponseDto
    {
        public string? Id { get; set; } = null;
        public string? ImageId { get; set; } = null;
        public string? UserName { get; set; } = null;
        public bool IsMember { get; set; }
    }

}