using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class PrctOfExpr : ExpressionBase
    {
        public ExpressionBase FirstOperand { get; }
        public ExpressionBase SecondOperand { get; }

        public PrctOfExpr(ExpressionBase firstOperand, ExpressionBase secondOperand)
        {
            FirstOperand = firstOperand;
            SecondOperand = secondOperand;
        }
    }
}