using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal abstract class EqualitySearchExpression : SearchExpression
    {
        protected EqualitySearchExpression(SearchConfigurationOptions options, string text) : base(options, text) { }

        public SearchExpression Left { get; protected set; }
        public SearchExpression Right { get; protected set; }

        internal override SearchExpression Decompose()
        {
            // EqualitySearchExpressions cannot be further decomposed
            return this;
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

            // on the left side, we will try to find a field we can compare to
            var entityConfig = Options.Entity<T>();

            // try explicit predicates first.
            var predicate = entityConfig.GetPredicate(Left.Text);
            if (predicate != null)
            {
                return GetParameterPredicateExpression(parameterExpression, predicate, entityConfig);
            }

            return GetParameterConstantExpression(parameterExpression, entityConfig);
        }

        private Expression<Func<T, bool>> GetParameterPredicateExpression<T>(
            ParameterExpression parameterExpression,
            Expression<Func<T, string, bool>> predicate,
            EntitySearchConfiguration<T> entityConfig)
        {
            var predicateCall = Expression.Invoke(predicate, parameterExpression, Expression.Constant(Right.Text));
            return Expression.Lambda<Func<T, bool>>(predicateCall, parameterExpression);

        }

        private Expression<Func<T, bool>> GetParameterConstantExpression<T>(
            ParameterExpression parameterExpression,
            EntitySearchConfiguration<T> entityConfig)
        {
            // we'll need this later in a couple places:
            var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);

            var searchField = entityConfig.GetTypeMapping(Left.Text);
            if (searchField == null)
            {
                throw new SearchParseException($"Unable to find a field with name { Left.Text }");
            }

            var oldParameter = searchField.Parameters.First();
            var convertParameter = new ParameterConversionVisitor(parameterExpression, oldParameter);
            var leftExpression = convertParameter.Visit(searchField.Body);

            // the right side is easy since it is a constant, 
            // however we have to try to force it to the correct type
            var leftType = leftExpression.Type;
            bool isEnumerable = false;
            if (leftType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(leftType.GetGenericTypeDefinition()))
            {
                isEnumerable = true;
                // if IEnumerable, get the underlying type
                leftType = leftType.GetGenericArguments().Single();
            }
            Expression rightExpression;
            if (leftType == typeof(string))
            {
                // special case for strings, we don't have to convert them
                // but we should uppercase them
                rightExpression = Expression.Call(
                    Expression.Constant(Right.Text),
                    toUpperMethod);
            }
            else
            {
                // any other types we have to convert
                var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(leftType);
                var rightValue = typeConverter.ConvertFromString(Right.Text);
                rightExpression = Expression.Constant(rightValue, leftType);
            }

            // now all that's left is the operator, which should be supplied by a base class
            Expression expr;
            // special case for IEnumerable types, we need to do an ANY
            if (isEnumerable)
            {
                var anyExpression = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(x => x.Name == "Any")
                    .Single(mi => mi.GetParameters().Count() == 2)
                    .MakeGenericMethod(leftType);
                var newParam = Expression.Parameter(leftType);
                var newLeftExpression = leftType == typeof(string) ? (Expression)Expression.Call(newParam, toUpperMethod) : newParam;

                var subExpression = CreateOperatorExpression<T>(newLeftExpression, rightExpression, parameterExpression);

                var func = Expression.Lambda(
                    subExpression,
                    newParam);

                expr =  Expression.Call(
                    anyExpression,
                    leftExpression,
                    func);
            }
            else
            {
                // don't forget to uppercase string!
                if (leftType == typeof(string))
                {
                    leftExpression = Expression.Call(leftExpression, toUpperMethod);
                }
                expr = CreateOperatorExpression<T>(leftExpression, rightExpression, parameterExpression);
            }


            return Expression.Lambda<Func<T, bool>>(expr, parameterExpression);

        }


        protected abstract Expression CreateOperatorExpression<T>(Expression left,
            Expression right,
            ParameterExpression parameterExpression);
    }
}
