using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class ResetRequestDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}
