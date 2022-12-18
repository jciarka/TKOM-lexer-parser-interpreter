using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class AndExpr : GrammarRuleBase, IExpression, IVisitable
    {
        public IExpression FirstOperand { get; }
        public IEnumerable<IExpression> Operands { get; }

        public AndExpr(IExpression firstOperand, IEnumerable<IExpression> operands, RulePosition position) : base(position)
        {
            FirstOperand = firstOperand;
            Operands = operands;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}