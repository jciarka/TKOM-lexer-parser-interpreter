using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class MultiplicativeExpr : ExpressionBase
    {
        public TokenType Operator { get; }
        public ExpressionBase FirstOperand { get; }
        public IEnumerable<ExpressionBase> Operands { get; }

        public MultiplicativeExpr(TokenType @operator, ExpressionBase firstOperand, IEnumerable<ExpressionBase> operands)
        {
            Operator = @operator;
            FirstOperand = firstOperand;
            Operands = operands;
        }
    }
}