using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class FinancialFromStmt : StatementBase
    {
        public TokenType Operator { get; }
        public ExpressionBase AccountFromExpression { get; }
        public ExpressionBase ValueExpression { get; }
        public ExpressionBase? AccountToExpression { get; }

        public FinancialFromStmt(ExpressionBase accountFromExpression, TokenType @operator, ExpressionBase valueExpression, ExpressionBase? accountToExpression = null)
        {
            AccountFromExpression = accountFromExpression;
            Operator = @operator;
            ValueExpression = valueExpression;
            AccountToExpression = accountToExpression;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
