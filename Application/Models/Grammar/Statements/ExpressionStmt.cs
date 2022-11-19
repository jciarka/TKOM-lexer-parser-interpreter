using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class ExpressionStmt : StatementBase
    {
        public ExpressionBase RightExpression { get; }

        public ExpressionStmt(ExpressionBase rValue)
        {
            RightExpression = rValue;
        }
    }
}
