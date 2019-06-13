using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class ContainsSearchExpression : EqualitySearchExpression
    {
        public ContainsSearchExpression(SearchConfigurationOptions options, string text) : base(options, text) { }

        protected override Expression CreateOperatorExpression<T>(Expression left,
            Expression right,
            ParameterExpression parameterExpression)
        {
            if (left.Type == typeof(string))
            {
                // we'll use the null-coalescing operator so we don't get null refernces
                var nullCoalesceExpression = Expression.Coalesce(left, Expression.Constant(string.Empty));

                return Expression.Call(nullCoalesceExpression,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    right);
            }
            else if (left.Type == typeof(IEnumerable<string>))
            {
                // this is harder because we have to do an ANY on the enumeration.

                var overload = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(x => x.Name == "Any")
                        .Single(mi => mi.GetParameters().Count() == 2)
                        .MakeGenericMethod(typeof(string));

                // this obviously doesn't work.
                //var containsMethod = Expression.Call(left,
                //        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                //        right);
                var newParam = Expression.Parameter(typeof(string));
                var newNullCoalesceExpression = Expression.Coalesce(newParam, Expression.Constant(string.Empty));

                var func = Expression.Lambda(
                    Expression.Call(newNullCoalesceExpression,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        right), newParam);

                var call = Expression.Call(
                    overload,
                    left,
                    func);

                return call;



                throw new NotImplementedException();
            }
            else if (left.Type == typeof(DateTime) && right.Type == typeof(DateTime))
            {
                // we will just compare the day in this case
                var dateTimeType = typeof(DateTime);
                var yearProperty = dateTimeType.GetProperty("Year");
                var monthProperty = dateTimeType.GetProperty("Month");
                var dayProperty = dateTimeType.GetProperty("Day");
                var yearExpression = Expression.Equal(
                    Expression.Property(left, yearProperty),
                    Expression.Property(right, yearProperty));
                var monthExpression = Expression.Equal(
                    Expression.Property(left, monthProperty),
                    Expression.Property(right, monthProperty));
                var dayExpression = Expression.Equal(
                    Expression.Property(left, dayProperty),
                    Expression.Property(right, dayProperty));

                return Expression.AndAlso(
                    Expression.And(yearExpression, monthExpression),
                    dayExpression);
            }
            else
            {
                throw new SearchParseException($"Cannot apply contains to field of type { left.Type.Name }");
            }
        }

        internal static ContainsSearchExpression Create(
            SearchConfigurationOptions options,
            SearchExpression left,
            SearchExpression right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            return new ContainsSearchExpression(options, right.Text)
            {
                Left = left,
                Right = right
            };
        }
    }
}
