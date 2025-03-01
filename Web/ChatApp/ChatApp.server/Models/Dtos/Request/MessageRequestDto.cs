using ChatApi.server.Validation;
using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    [AtLeastOneRequired(nameof(Content), nameof(Files))]
    public class MessageRequestDto
    {
        public string? Content { get; set; }
        public ICollection<IFormFile>? Files { get; set; }
    }
}
