using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class BracedExprTerm : TermBase
    {
        public ExpressionBase Expression { get; }

        public BracedExprTerm(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
