using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class BracedExprTerm : GrammarRuleBase, ITerm, IVisitable
    {
        public IExpression Expression { get; }

        public BracedExprTerm(IExpression expression, RulePosition position) : base(position)
        {
            Expression = expression;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
