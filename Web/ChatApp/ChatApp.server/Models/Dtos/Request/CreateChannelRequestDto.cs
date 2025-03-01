using ChatApi.server.Models.DbSet;
using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{
    public class CreateChannelRequestDto
    {
        [StringLength(50, MinimumLength = 1)]
        public string name { get; set; }
        public eChannelType? channelType { get; set; }
        public string? category { get; set; }
    }
}
