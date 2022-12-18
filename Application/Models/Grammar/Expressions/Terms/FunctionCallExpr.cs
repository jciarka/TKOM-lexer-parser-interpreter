using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class FunctionCallExpr : GrammarRuleBase, ITerm, IVisitable
    {
        public string Name { get; }
        public IEnumerable<IArgument> Arguments { get; }

        public FunctionCallExpr(string name, IEnumerable<IArgument> arguments, RulePosition position) : base(position)
        {
            Name = name;
            Arguments = arguments;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}