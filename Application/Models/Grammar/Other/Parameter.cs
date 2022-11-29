using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public class Parameter : GrammarRuleBase
    {
        public string Type { get; }
        public string Identifier { get; set; }

        public Parameter(string type, string identifier)
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