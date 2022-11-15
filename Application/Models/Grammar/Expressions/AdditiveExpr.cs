using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class AdditiveExpr : ExpressionBase
    {
        public TokenType Operator { get; }
        public ExpressionBase FirstOperand { get; }
        public IEnumerable<ExpressionBase> Operands { get; }

        public AdditiveExpr(TokenType @operator, ExpressionBase firstOperand, IEnumerable<ExpressionBase> operands)
        {
            Operator = @operator;
            FirstOperand = firstOperand;
            Operands = operands;
        }
    }
}
