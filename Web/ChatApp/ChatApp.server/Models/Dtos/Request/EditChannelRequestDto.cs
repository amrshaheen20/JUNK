using ChatApi.server.Models.DbSet;
using ChatApi.server.Validation;
using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    [AtLeastOneRequired(nameof(Name), nameof(Type))]
    public class EditChannelRequestDto
    {
        [StringLength(50, MinimumLength = 1)]
        public string? Name { get; set; }
        public string? Category { get; set; }
    }
}
