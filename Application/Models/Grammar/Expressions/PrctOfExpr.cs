using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class PrctOfExpr : GrammarRuleBase, IExpression, IVisitable
    {
        public IExpression FirstOperand { get; }
        public IExpression SecondOperand { get; }

        public PrctOfExpr(IExpression firstOperand, IExpression secondOperand, RulePosition position) : base(position)
        {
            FirstOperand = firstOperand;
            SecondOperand = secondOperand;
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