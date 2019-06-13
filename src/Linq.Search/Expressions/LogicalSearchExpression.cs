using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class LogicalSearchExpression : SearchExpression
    {
        protected LogicalSearchExpression(SearchConfigurationOptions options, string text) : base(options, text) { }

        public SearchExpression Left { get; private set; }
        public SearchExpression Right { get; private set; }
        public Operator Operator { get; private set; }

        internal override SearchExpression Decompose()
        {
            var leftDecomposed = Left.Decompose();
            var rightDecomposed = Right.Decompose();
            return leftDecomposed != Left || rightDecomposed != Right ? Create(Options, leftDecomposed, rightDecomposed, Operator, Text) : (this);
        }

        internal override Expression<Func<T, bool>> ToWhereClause<T>(ParameterExpression parameterExpression)
        {
            if (Left == null)
            {
                if (Right == null)
                {
                    return base.ToWhereClause<T>(parameterExpression);
                }
                else
                {
                    throw new InvalidOperationException("The left expression of this logical expression cannot be null");
                }
            }
            else if (Right == null)
            {
                throw new InvalidOperationException("The right expression of this logical expression cannot be null");
            }

            Expression leftExpression1 = Left.ToWhereClause<T>(parameterExpression);
            Expression rightExpression1 = Right.ToWhereClause<T>(parameterExpression);
            var leftExpression2 = ((LambdaExpression)leftExpression1).Body;
            var rightExpression2 = ((LambdaExpression)rightExpression1).Body;
            Expression logicalExpression;
            if (Operator == Operator.And)
            {
                logicalExpression = Expression.And(leftExpression2, rightExpression2);
            }
            else if (Operator == Operator.Or)
            {
                logicalExpression = Expression.Or(leftExpression2, rightExpression2);
            }
            else
            {
                throw new InvalidOperationException($"Unknown operator { Operator }");
            }

            return Expression.Lambda<Func<T, bool>>(logicalExpression, parameterExpression);
        }


        internal static LogicalSearchExpression Create(
            SearchConfigurationOptions options,
            SearchExpression left,
            SearchExpression right,
            Operator @operator,
            string text)
        {
            return new LogicalSearchExpression(options, text)
            {
                Left = left,
                Right = right,
                Operator = @operator
            };
        }
    }
}
