using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class PasswordResetRequestDto
    {
        public string UserId { get; set; }

        [StringLength(127, MinimumLength = 8)]
        public string Password { get; set; }

        public string Token { get; set; }
    }
}
