using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public class ConstructiorCallExpr : TermBase
    {
        public string Type { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public ConstructiorCallExpr(string type, IEnumerable<ArgumentBase> arguments)
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