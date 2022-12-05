using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class Parameter : GrammarRuleBase
    {
        public TypeBase Type { get; }
        public string Identifier { get; set; }

        public Parameter(TypeBase type, string identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}