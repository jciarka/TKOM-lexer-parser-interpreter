using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System.Linq.Expressions;

namespace Application.Models.Grammar
{
    public class FinancialToStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public TokenType Operator { get; }
        public IExpression AccountExpression { get; }
        public IExpression ValueExpression { get; }

        public FinancialToStmt(IExpression accountExpression, TokenType @operator, IExpression valueExpression, RulePosition position) : base(position)
        {
            Operator = @operator;
            ValueExpression = valueExpression;
            AccountExpression = accountExpression;
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
