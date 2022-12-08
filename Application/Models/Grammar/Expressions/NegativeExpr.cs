using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class NegativeExpr : ExpressionBase
    {
        public TokenType Operator { get; }
        public ExpressionBase Operand { get; }

        public NegativeExpr(TokenType @operator, ExpressionBase operand)
        {
            Operand = operand;
            Operator = @operator;
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
