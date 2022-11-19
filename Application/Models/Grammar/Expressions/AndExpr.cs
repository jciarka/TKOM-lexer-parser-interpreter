using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class AndExpr : ExpressionBase
    {
        public ExpressionBase FirstOperand { get; }
        public IEnumerable<ExpressionBase> Operands { get; }

        public AndExpr(ExpressionBase firstOperand, IEnumerable<ExpressionBase> operands)
        {
            FirstOperand = firstOperand;
            Operands = operands;
        }
    }
}