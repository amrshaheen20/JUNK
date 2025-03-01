using ChatApi.server.Validation;
using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    [AtLeastOneRequired(nameof(name), nameof(Image))]
    public class EditServerRequestDto
    {
        [StringLength(50, MinimumLength = 1)]
        public string? name { get; set; }
        public IFormFile? Image { get; set; }
    }
}
