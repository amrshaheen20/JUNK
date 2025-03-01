using ChatApi.server.Models.DbSet;
using System.Linq.Expressions;

namespace ChatApi.server.Extensions
{
    public enum ePaginateSearchDirection
    {
        Before,
        After,
        Around
    }

    public class PaginateBlock<T>
    {
        public int TotalCount { get; set; }
        public T? Data { get; set; }

        public PaginateBlock() { }

        public PaginateBlock(int total, T data)
        {
            TotalCount = total;
            Data = data;
        }
    }

    public static class Paginate
    {
        public static IQueryable<T> PaginateByCursor<T, TKey>(
            this IQueryable<T> query,
            string? cursor,
            ePaginateSearchDirection direction,
            int pageSize,
            Expression<Func<T, TKey>> orderBySelector,
            bool FromNewToOld = true
            ) where T : BaseEntity
        {

#if DEBUG 
            if (pageSize == -1)
            {
                pageSize = query.Count();
            }
            else
            {
                pageSize = Math.Clamp(pageSize, 1, Constants.MAX_QUERY_LIMIT);
            }
#else
                pageSize = Math.Clamp(pageSize, 1, Constants.MAX_QUERY_LIMIT);
#endif


            if (!string.IsNullOrEmpty(cursor))
            {
                var targetItem = query.Where(m => m.Id == cursor)
                                        .Select(m => new { m.CreatedAt })
                                        .FirstOrDefault();

                if (targetItem != null)
                {
                    if (direction == ePaginateSearchDirection.Before)
                    {
                        query = (FromNewToOld ? query.OrderByDescending(orderBySelector) : query.OrderBy(orderBySelector));
                        query = query.Where(x => x.CreatedAt < targetItem.CreatedAt); //before
                        return query.Take(pageSize);
                    }
                    else if (direction == ePaginateSearchDirection.After)
                    {
                        query = (FromNewToOld ? query.OrderBy(orderBySelector) : query.OrderByDescending(orderBySelector));
                        query = query.Where(x => x.CreatedAt > targetItem.CreatedAt); // after

                        return (FromNewToOld ? query.Take(pageSize).OrderByDescending(orderBySelector) : query.Take(pageSize).OrderBy(orderBySelector));
                    }
                    else if (direction == ePaginateSearchDirection.Around)
                    {
                        int halfLimit = Math.Max(pageSize / 2, 1);

                        var beforeQuery = (FromNewToOld ?
                            query.Where(x => x.CreatedAt <= targetItem.CreatedAt).OrderByDescending(orderBySelector)
                            : query.Where(x => x.CreatedAt <= targetItem.CreatedAt).OrderBy(orderBySelector))
                                               .Take(halfLimit); // Fetch half before

                        var afterQuery = (FromNewToOld ? query.Where(x => x.CreatedAt > targetItem.CreatedAt)
                                              .OrderBy(orderBySelector)
                                              : query.Where(x => x.CreatedAt > targetItem.CreatedAt)
                                              .OrderByDescending(orderBySelector))
                                              .Take(halfLimit); // Fetch half after

                        return (FromNewToOld ? beforeQuery.Union(afterQuery).OrderByDescending(orderBySelector) : beforeQuery.Union(afterQuery).OrderBy(orderBySelector));
                    }
                }
            }

            return (FromNewToOld ? query.OrderByDescending(orderBySelector).Take(pageSize) : query.OrderBy(orderBySelector).Take(pageSize));
        }

        public static PaginateBlock<IQueryable<T>> PaginateByCursorWithCounter<T, TKey>(
            this IQueryable<T> query,
            string? cursor,
            ePaginateSearchDirection direction,
            int pageSize,
            Expression<Func<T, TKey>> orderBySelector,
            bool FromNewToOld = true
            ) where T : BaseEntity
        {

            return new PaginateBlock<IQueryable<T>>(query.Count(), PaginateByCursor(query, cursor, direction, pageSize, orderBySelector, FromNewToOld));
        }


        public static IQueryable<T> PaginateByPage<T>(this IQueryable<T> source, int page, int pageSize)
        {

#if DEBUG
            if (pageSize == -1)
            {
                return source;
            }
#endif

            if (pageSize > 0)
                pageSize = Math.Clamp(pageSize, 1, Constants.MAX_QUERY_LIMIT);

            page = Math.Max(1, page);

            return source.Skip((page - 1) * pageSize).Take(pageSize);

        }


        public static PaginateBlock<IQueryable<T>> PaginateByPageWithCounter<T>(this IQueryable<T> source, int page, int pageSize)
        {
            return new PaginateBlock<IQueryable<T>>(source.Count(), PaginateByPage(source, page, pageSize));
        }
    }

}