using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class UpdateProfileRequestDto
    {
        [RegularExpression("^(?=[a-zA-Z0-9._]{8,20}$)(?!.*[_.]{2})[^_.].*[^_.]$", ErrorMessage = "Invalid Username")]
        public string? UserName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [StringLength(127, MinimumLength = 1)]
        public string? DisplayName { get; set; }
        public IFormFile? Image { get; set; }
        [StringLength(127, MinimumLength = 0)]
        public string? Bio { get; set; }
    }
}
