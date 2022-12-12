using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class FinancialToStmt : StatementBase
    {
        public TokenType Operator { get; }
        public ExpressionBase AccountExpression { get; }
        public ExpressionBase ValueExpression { get; }

        public FinancialToStmt(ExpressionBase accountExpression, TokenType @operator, ExpressionBase valueExpression, RulePosition position) : base(position)
        {
            Operator = @operator;
            ValueExpression = valueExpression;
            AccountExpression = accountExpression;
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
