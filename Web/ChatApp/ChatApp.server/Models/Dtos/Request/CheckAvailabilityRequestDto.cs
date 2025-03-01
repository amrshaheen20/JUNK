using ChatApi.server.Validation;
using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    [AtLeastOneRequired(nameof(Email), nameof(UserName))]
    public class CheckAvailabilityRequestDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        [RegularExpression("^(?=[a-zA-Z0-9._]{8,20}$)(?!.*[_.]{2})[^_.].*[^_.]$", ErrorMessage = "Invalid Username")]
        public string? UserName { get; set; }
    }

}
