using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class ConstructiorCallExpr : GrammarRuleBase, ITerm, IVisitable
    {
        public TypeBase Type { get; }
        public IEnumerable<IArgument> Arguments { get; }

        public ConstructiorCallExpr(TypeBase type, IEnumerable<IArgument> arguments, RulePosition position) : base(position)
        {
            Type = type;
            Arguments = arguments;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}