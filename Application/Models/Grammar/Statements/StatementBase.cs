using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public abstract class StatementBase : GrammarRuleBase
    {
        public abstract void Accept(IPresenterVisitor visitor, int v);
    }
}