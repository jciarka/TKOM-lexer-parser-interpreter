using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class AdditiveExpr : GrammarRuleBase, IExpression, IVisitable
    {
        public IExpression FirstOperand { get; }
        public IEnumerable<Tuple<TokenType, IExpression>> Operands { get; }

        public AdditiveExpr(IExpression firstOperand, IEnumerable<Tuple<TokenType, IExpression>> operands, RulePosition position) : base(position)
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
