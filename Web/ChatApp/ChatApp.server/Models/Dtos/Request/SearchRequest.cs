using System.ComponentModel.DataAnnotations;

namespace ChatApi.server.Models.Dtos.Request
{

    public class SearchRequest: PageRequest
    {
        /// <summary>
        /// The search term used for filtering results.
        /// </summary>
        [Required]
        [StringLength(127, MinimumLength = 1)]
        public string Query { get; set; } = string.Empty;

    }
}
