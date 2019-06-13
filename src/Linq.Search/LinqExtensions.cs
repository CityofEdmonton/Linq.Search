using System;
using System.Linq;
using System.Linq.Expressions;
using CityofEdmonton.Linq.Search;
using CityofEdmonton.Linq.Search.Expressions;

namespace System.Linq
{ 
    public static class SearchExtensions
    { 
        public static IQueryable<T> Search<T>(this IQueryable<T> items, string searchText)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return items;
            }

            // parse searchText
            var searchExpression = SearchExpression.Create(SearchConfiguration.CurrentConfig, searchText)
                .Decompose();


            return items.Where(searchExpression.ToWhereClause<T>(Expression.Parameter(typeof(T))));
        }


    }
}
