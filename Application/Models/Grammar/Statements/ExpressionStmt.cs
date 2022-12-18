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

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
