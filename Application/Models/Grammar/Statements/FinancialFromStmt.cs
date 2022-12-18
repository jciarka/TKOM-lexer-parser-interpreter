using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class FinancialFromStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public TokenType Operator { get; }
        public IExpression AccountFromExpression { get; }
        public IExpression ValueExpression { get; }
        public IExpression? AccountToExpression { get; }

        public FinancialFromStmt(IExpression accountFromExpression, TokenType @operator, IExpression valueExpression, RulePosition position, IExpression? accountToExpression = null) : base(position)
        {
            AccountFromExpression = accountFromExpression;
            Operator = @operator;
            ValueExpression = valueExpression;
            AccountToExpression = accountToExpression;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
