using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public abstract class StatementBase : GrammarRuleBase
    {
        public abstract void Accept(IPresenterVisitor visitor, int v);
        public abstract TypeBase Accept(ITypingAnalyseVisitor visitor);
    }
}