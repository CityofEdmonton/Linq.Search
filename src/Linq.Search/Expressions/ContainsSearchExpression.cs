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
            // we'll need this later in a couple places:
            var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);

            // we'll do our Contains() case insensitive, might as well Upper() the right side now
            var toUpperExpressionRight = Expression.Call(right, toUpperMethod);


            if (left.Type == typeof(string))
            {
                // we'll use the null-coalescing operator so we don't get null refernces
                var nullCoalesceExpression = Expression.Coalesce(left, Expression.Constant(string.Empty));

                // and do this case-insensitive (ToUpper() both sides)
                var toUpperExpressionLeft = Expression.Call(nullCoalesceExpression, toUpperMethod);

                return Expression.Call(Expression.Call(nullCoalesceExpression, toUpperMethod),
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    toUpperExpressionRight);
            }
            else if (left.Type == typeof(IEnumerable<string>))
            {
                // this is harder because we have to do an ANY on the enumeration.

                var anyExpression = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(x => x.Name == "Any")
                        .Single(mi => mi.GetParameters().Count() == 2)
                        .MakeGenericMethod(typeof(string));

                var newParam = Expression.Parameter(typeof(string));
                var newNullCoalesceExpression = Expression.Coalesce(newParam, Expression.Constant(string.Empty));
                var toUpperExpressionLeft = Expression.Call(newNullCoalesceExpression, toUpperMethod);

                var func = Expression.Lambda(
                    Expression.Call(toUpperExpressionLeft,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        toUpperExpressionRight),
                    newParam);

                return Expression.Call(
                    anyExpression,
                    left,
                    func);
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
