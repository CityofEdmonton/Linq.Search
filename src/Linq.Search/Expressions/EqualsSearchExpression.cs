using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class EqualsSearchExpression : EqualitySearchExpression
    {
        public EqualsSearchExpression(SearchConfigurationOptions options, string text) : base(options, text) { }

        protected override Expression CreateOperatorExpression<T>(Expression left,
            Expression right,
            ParameterExpression parameterExpression)
        {
            return Expression.Equal(left, right);
        }

        internal static EqualsSearchExpression Create(
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

            return new EqualsSearchExpression(options, right.Text)
            {
                Left = left,
                Right = right
            };
        }

    }
}
