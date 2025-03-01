using ChatApi.server.Extensions;

namespace ChatApi.server.Models.Dtos.Request
{

    public class PaginateRequest
    {
        /// <summary>
        /// the cursor for retrieving items before a specific point.
        /// </summary>
        public string? Before { get; set; }

        /// <summary>
        /// the cursor for retrieving items after a specific point.
        /// </summary>
        public string? After { get; set; }

        /// <summary>
        /// the cursor for retrieving items around a specific point.
        /// </summary>
        public string? Around { get; set; }

        /// <summary>
        /// Specifies the maximum number of items to retrieve.  
        /// The maximum allowed value is 50.  
        /// </summary>
        public int Limit { get; set; } = Constants.MAX_QUERY_LIMIT;

        /// <summary>
        /// to include the total count of items in the response.  
        /// Default is `false`
        /// </summary>
        public bool ShowTotalNumber { get; set; } = false;

        public ePaginateSearchDirection GetDirection()
        {
            if (Before != null)
                return ePaginateSearchDirection.Before;

            if (After != null)
                return ePaginateSearchDirection.After;

            if (Around != null)
                return ePaginateSearchDirection.Around;

            return ePaginateSearchDirection.Before;
        }

        public string? GetCursor()
        {
            return Before ?? After ?? Around;
        }
    }
}
