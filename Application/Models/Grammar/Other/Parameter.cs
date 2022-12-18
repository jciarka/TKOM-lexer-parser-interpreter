using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class Parameter : GrammarRuleBase, IVisitable
    {
        public TypeBase Type { get; }
        public string Identifier { get; set; }

        public Parameter(TypeBase type, string identifier, RulePosition position) : base(position)
        {
            Type = type;
            Identifier = identifier;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}