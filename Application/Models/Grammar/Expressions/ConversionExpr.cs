using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class ConversionExpr : ExpressionBase
    {
        public ExpressionBase OryginalExpression { get; }
        public ExpressionBase TypeExpression { get; }

        public ConversionExpr(ExpressionBase oryginalExpression, ExpressionBase typeExpression)
        {
            OryginalExpression = oryginalExpression;
            TypeExpression = typeExpression;
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