using System.Linq.Expressions;

namespace ChatApi.server.Extensions
{

    public static class IQueryableExtensions
    {
        public static IQueryable<T> Search<T, TKey>(
            this IQueryable<T> source,
            string searchTerm,
            Expression<Func<T, TKey>> orderBySelector,
            params Expression<Func<T, string>>[] properties
            )
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || properties.Length == 0)
            {
                return source;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var words = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Expression? anyWordExpression = null;
            //Expression? fullTextExpression = null;

            foreach (var property in properties)
            {
                var propertyExpression = Expression.Invoke(property, parameter);

                Expression propertyToSearch = propertyExpression;


                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                propertyToSearch = Expression.Call(propertyExpression, toLowerMethod!);


                //var fullTextConstant = Expression.Constant(searchTerm.ToLower());
                //var equalsMethod = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                //var equalsExpression = Expression.Call(propertyToSearch, equalsMethod!, fullTextConstant);
                //fullTextExpression = fullTextExpression == null
                //    ? equalsExpression
                //    : Expression.OrElse(fullTextExpression, equalsExpression);




                foreach (var word in words)
                {
                    var wordConstant = Expression.Constant(word.ToLower());

                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var containsExpression = Expression.Call(propertyToSearch, containsMethod!, wordConstant);

                    anyWordExpression = anyWordExpression == null
                        ? containsExpression
                        : Expression.OrElse(anyWordExpression, containsExpression);
                }
            }

            var anyWordLambda = Expression.Lambda<Func<T, bool>>(anyWordExpression!, parameter);

            //var fullTextLambda = Expression.Lambda<Func<T, bool>>(fullTextExpression!, parameter);

            var matchingResults = source.OrderByDescending(orderBySelector).Where(anyWordLambda);

            return matchingResults;
        }
    }
}

