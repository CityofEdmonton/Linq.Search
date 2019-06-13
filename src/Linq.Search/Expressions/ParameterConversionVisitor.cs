using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CityofEdmonton.Linq.Search.Expressions
{
    internal class ParameterConversionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression newParameter;
        private readonly ParameterExpression oldParameter;

        public ParameterConversionVisitor(ParameterExpression newParameter, ParameterExpression oldParameter)
        {
            this.newParameter = newParameter;
            this.oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == oldParameter)
                return newParameter; // replace all old param references with new ones
            else
                return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj = Visit(node.Expression);
            var newMember = newParameter.Type.GetMember(node.Member.Name).First();
            return Expression.MakeMemberAccess(newObj, newMember);
        }
    }
}
