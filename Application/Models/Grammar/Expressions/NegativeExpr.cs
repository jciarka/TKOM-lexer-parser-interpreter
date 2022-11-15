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
    }
}
