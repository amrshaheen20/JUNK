using ChatApi.server.Extensions;

namespace ChatApi.server.Models.Dtos.Request
{

    public class PageRequest
    {
        /// <summary>
        /// Specifies the maximum number of items to retrieve.
        /// </summary>
        public int Limit { get; set; } = Constants.MAX_QUERY_LIMIT;

        /// <summary>
        /// The page number for paginated results.
        /// First page is the default one.
        /// </summary>
        public int PageNumber { get; set; } = 1;
    }
}
