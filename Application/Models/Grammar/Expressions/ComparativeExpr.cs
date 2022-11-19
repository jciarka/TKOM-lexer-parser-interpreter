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
    }
}