using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class LoginRequestDto
    {
        public string Email { get; set; }

        [StringLength(127, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
