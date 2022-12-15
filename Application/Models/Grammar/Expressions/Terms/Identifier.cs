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