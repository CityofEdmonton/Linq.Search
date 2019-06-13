using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class LessThanSearchExpression : EqualitySearchExpression
    {
        public LessThanSearchExpression(SearchConfigurationOptions options, string text) : base(options, text) { }

        protected override Expression CreateOperatorExpression<T>(Expression left, Expression right, ParameterExpression parameterExpression)
        {
            return Expression.LessThan(left, right);
        }

        internal static LessThanSearchExpression Create(
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

            return new LessThanSearchExpression(options, right.Text)
            {
                Left = left,
                Right = right
            };
        }
    }
}
