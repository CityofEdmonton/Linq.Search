using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class SearchExpression
    {
        protected SearchExpression(SearchConfigurationOptions options, string text)
        {
            Options = options;
            Text = text;
        }
        protected SearchConfigurationOptions Options { get; private set; }

        public string Text { get; }

        internal static SearchExpression Create(SearchConfigurationOptions options, string text)
        {
            return new SearchExpression(options, text);
        }

        public override string ToString()
        {
            return Text;
        }

        // attempt to break this expression down to smaller components,
        // like LogicalSearchExpressions or EqualsExpressions, etc.
        // returns the same object if not able to.
        internal virtual SearchExpression Decompose()
        {
            // first look for logical operators
            var decomposed = DecomposeLogicalOperators();
            if (decomposed != this)
            {
                // further break it down
                decomposed = decomposed.Decompose();
            }
            else
            {
                // let's see what we can do with what we've got left.
                decomposed = DecomposeEqualityOperators();

                // I don't think it's possible to further break this down, 
                // so we'll stop here.
            }

            return decomposed;
        }


        internal virtual Expression<Func<T, bool>> ToWhereClause<T>(ParameterExpression parameterExpression)
        {
            // default implementation is to search all fields specified by the 
            // DefaultSearchField option in SearchConfigurationOptions
            var entityConfig = Options.Entity<T>();
            if (entityConfig.DefaultSearchFields == null || !entityConfig.DefaultSearchFields.Any())
            {
                throw new InvalidOperationException($"There are no DefaultSearchFields specified for the enity of type { typeof(T).FullName }");
            }

            Expression expr = null;
            foreach (var searchField in entityConfig.DefaultSearchFields)
            {
                Expression currentExpression;
                var oldParameter = searchField.Parameters.First();
                var convertParameter = new ParameterConversionVisitor(parameterExpression, oldParameter);
                var newBody = convertParameter.Visit(searchField.Body);
                if (searchField.ReturnType == typeof(string))
                {
                    // we'll use the null-coalescing operator so we don't get null refernces
                    var nullCoalesceExpression = Expression.Coalesce(newBody, Expression.Constant(string.Empty));

                    currentExpression = Expression.Call(nullCoalesceExpression,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(Text, typeof(string)));
                }
                else
                {
                    throw new InvalidOperationException("Currently only expressions of type string are supported");
                }

                if (expr == null)
                {
                    expr = currentExpression;
                }
                else
                {
                    // we combine the previous expression with the current in an OR clause
                    expr = Expression.Or(expr, currentExpression);
                }

            }

            return Expression.Lambda<Func<T, bool>>(expr, parameterExpression);
        }



        static SearchExpression()
        {
            var buildOperatorSearchRegex = new Func<IEnumerable<string>, Regex>(operators =>
            {
                var sb = new StringBuilder();
                var first = true;
                foreach (var s in operators)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append("|");
                    }

                    sb.Append("(\\b\\s*");
                    sb.Append(s);
                    sb.Append("\\s*\\b)");
                }
                return new Regex(sb.ToString(), RegexOptions.IgnoreCase);
            });

            _binaryOperatorsSearch = buildOperatorSearchRegex(_andOperators.Concat(_orOperators));
            _equalityOperatorsSearch = buildOperatorSearchRegex(_equalityOperators);
        }

        #region DecomposeLogicalOperators
        private static readonly string[] _andOperators = new string[] { "AND", "\\&" };
        private static readonly string[] _orOperators = new string[] { "OR", "\\|" };
        private static readonly Regex _binaryOperatorsSearch;

        /// <summary>
        /// Attemps to create LogicalSearchExpressions from the given text based on
        /// binary operators found in the text (AND, OR, etc.).
        /// If it finds them it will return a LogicalSearchPression with Left and Right 
        /// filled in and the Text will contain whatever is left over.
        /// If it cannot find any binary operators it will just return the original 
        /// searchExpression.
        /// </summary>
        /// <returns></returns>
        private SearchExpression DecomposeLogicalOperators()
        {
            var match = _binaryOperatorsSearch.Match(Text);
            if (!match.Success)
            {
                // easy case first.
                return this;
            }
            else
            {
                var searchExpressions = new Stack<SearchExpression>();
                var operators = new Stack<Operator>();

                // we go left to right through the matches.
                for (var i = 0; i < match.Captures.Count; i++)
                {
                    var previous = i > 0 ? match.Captures[i - 1] : null;
                    var current = match.Captures[i];
                    var next = i < match.Captures.Count - 1 ? match.Captures[i + 1] : null;
                    //var nextNext = i < match.Captures.Count - 2 ? match.Captures[i + 2] : null;

                    var leftStart = previous == null ? 0 : previous.Index + previous.Length;
                    var leftLength = current.Index - leftStart;
                    if (leftLength > 0)
                    {
                        searchExpressions.Push(SearchExpression.Create(Options, Text.Substring(leftStart, leftLength)));
                        Operator o;
                        var currentValue = current.Value.Trim();
                        if (_andOperators.Contains(currentValue, StringComparer.OrdinalIgnoreCase))
                        {
                            o = Operator.And;
                        }
                        else if (_orOperators.Contains(currentValue, StringComparer.OrdinalIgnoreCase))
                        {
                            o = Operator.Or;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown operator '{currentValue}'");
                        }

                        operators.Push(o);
                    }

                    if (next == null)
                    {
                        // this is the last one
                        var rightStart = current.Index + current.Length;
                        var rightLength = Text.Length - rightStart;
                        SearchExpression leftOver;
                        if (rightLength <= 0)
                        {
                            if (searchExpressions.Count > 0)
                            {
                                // nothing on the right side of the AND or OR,
                                // so we take the last left and just make it a regular SearchExpression
                                leftOver = searchExpressions.Pop();
                                operators.Pop();
                            }
                            else
                            {
                                // If here we didn't find anything at all
                                leftOver = SearchExpression.Create(Options, string.Empty);
                            }
                        }
                        else
                        {
                            leftOver = SearchExpression.Create(Options, Text.Substring(rightStart, rightLength).Trim());
                        }

                        // now we should have one more in the searchExpressions stack than in the operators stack
                        var searchExpressionCount = searchExpressions.Count;
                        if (searchExpressionCount == 0)
                        {
                            return leftOver;
                        }
                        else if (searchExpressionCount == 1)
                        {
                            return LogicalSearchExpression.Create(Options, searchExpressions.Pop(), leftOver, operators.Pop(), Text);
                        }
                        else
                        {
                            // if here there is at least 2 operators which we will make into a tree
                            var right = searchExpressions.Pop();
                            var left = searchExpressions.Pop();
                            var op = operators.Pop();
                            var currentExpression = LogicalSearchExpression.Create(
                                Options,
                                left,
                                right,
                                op,
                                text: $"{left.Text} {op} {right.Text}");


                            while (searchExpressions.Any())
                            {
                                right = currentExpression;
                                left = searchExpressions.Pop();
                                op = operators.Pop();
                                currentExpression = LogicalSearchExpression.Create(
                                    Options,
                                    left,
                                    right,
                                    op,
                                    text: $"{left.Text} {op} {right.Text}");
                            }

                            return currentExpression;
                        }
                    }
                }

                // if we get here we made a logical error in our programming
                throw new InvalidProgramException("Error parsing logical operators from search text");
            }
        }
        #endregion

        #region DecomposeEqualityOperators
        private static readonly string[] _equalityOperators = new string[] { "=", ":", ">=", "<=", "<", ">", "!=", "<>" };
        private static readonly Regex _equalityOperatorsSearch;
        private SearchExpression DecomposeEqualityOperators()
        {
            var match = _equalityOperatorsSearch.Match(Text);
            if (!match.Success)
            {
                // easy case first.
                return this;
            }
            else
            {
                var left = Create(Options, match.Index > 0 ? Text.Substring(0, match.Index) : string.Empty);
                var right = Create(Options, match.Index < Text.Length ? Text.Substring(match.Index + match.Length) : string.Empty);
                switch (match.Value.Trim())
                {
                    case "=":
                        return EqualsSearchExpression.Create(Options, left, right);
                    case ">=":
                        return GreaterThanOrEqualSearchExpression.Create(Options, left, right);
                    case ">":
                        return GreaterThanSearchExpression.Create(Options, left, right);
                    case "<=":
                        return LessThanOrEqualSearchExpression.Create(Options, left, right);
                    case "<":
                        return LessThanSearchExpression.Create(Options, left, right);
                    default:
                        return ContainsSearchExpression.Create(Options, left, right);
                }
            }
        }
        #endregion
    }
}
