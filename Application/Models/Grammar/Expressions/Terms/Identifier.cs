using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class Identifier : GrammarRuleBase, ITerm, IVisitable
    {
        public string Name { get; }

        public Identifier(string name, RulePosition position) : base(position)
        {
            Name = name;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}