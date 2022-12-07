using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
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

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase? Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
