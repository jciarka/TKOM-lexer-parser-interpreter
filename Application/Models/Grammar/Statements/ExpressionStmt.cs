using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class ExpressionStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public IExpression RightExpression { get; }

        public ExpressionStmt(IExpression rValue, RulePosition position) : base(position)
        {
            RightExpression = rValue;
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
