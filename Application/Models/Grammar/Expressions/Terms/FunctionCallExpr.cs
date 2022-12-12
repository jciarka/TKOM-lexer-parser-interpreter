using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class FunctionCallExpr : TermBase
    {
        public string Name { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public FunctionCallExpr(string name, IEnumerable<ArgumentBase> arguments, RulePosition position) : base(position)
        {
            Name = name;
            Arguments = arguments;
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