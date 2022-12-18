using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class ConversionExpr : GrammarRuleBase, IExpression, IVisitable
    {
        public IExpression OryginalExpression { get; }
        public IExpression TypeExpression { get; }

        public ConversionExpr(IExpression oryginalExpression, IExpression typeExpression, RulePosition position) : base(position)
        {
            OryginalExpression = oryginalExpression;
            TypeExpression = typeExpression;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}