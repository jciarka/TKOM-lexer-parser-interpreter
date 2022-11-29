using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public class Identifier : TermBase
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}