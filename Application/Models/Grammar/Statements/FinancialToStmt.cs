using Application.Infrastructure.Presenters;
using Application.Models.Tokens;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class FinancialToStmt : StatementBase
    {
        public TokenType Operator { get; }
        public ExpressionBase AccountExpression { get; }
        public ExpressionBase ValueExpression { get; }

        public FinancialToStmt(ExpressionBase accountExpression, TokenType @operator, ExpressionBase valueExpression)
        {
            Operator = @operator;
            ValueExpression = valueExpression;
            AccountExpression = accountExpression;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
