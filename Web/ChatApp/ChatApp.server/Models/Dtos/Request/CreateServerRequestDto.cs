using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class CreateServerRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string name { get; set; }
        public IFormFile? Image { get; set; }
    }
}
