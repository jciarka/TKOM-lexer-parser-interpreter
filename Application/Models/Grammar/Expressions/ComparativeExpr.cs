using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class ComparativeExpr : ExpressionBase
    {
        public ExpressionBase FirstOperand { get; }
        public IEnumerable<Tuple<TokenType, ExpressionBase>> Operands { get; }

        public ComparativeExpr(ExpressionBase firstOperand, IEnumerable<Tuple<TokenType, ExpressionBase>> operands)
        {
            FirstOperand = firstOperand;
            Operands = operands;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}