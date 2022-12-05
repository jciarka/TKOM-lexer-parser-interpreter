using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class ConstructiorCallExpr : TermBase
    {
        public TypeBase Type { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public ConstructiorCallExpr(TypeBase type, IEnumerable<ArgumentBase> arguments)
        {
            Type = type;
            Arguments = arguments;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}