using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(127, MinimumLength = 1)]
        public string DisplayName { get; set; }
        [Required]

        [RegularExpression("^(?=[a-zA-Z0-9._]{8,20}$)(?!.*[_.]{2})[^_.].*[^_.]$", ErrorMessage = "Invalid Username")]
        public string UserName { get; set; }

        public IFormFile? Image { get; set; }

        [Required]
        [StringLength(127, MinimumLength = 8)]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
