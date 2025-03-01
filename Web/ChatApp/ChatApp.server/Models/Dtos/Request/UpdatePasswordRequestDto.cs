using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class UpdatePasswordRequestDto
    {

        [StringLength(127, MinimumLength = 8)]
        public string OldPassword { get; set; }

        [StringLength(127, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}
