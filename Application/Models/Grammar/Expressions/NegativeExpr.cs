using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class NegativeExpr : GrammarRuleBase, IExpression, IVisitable
    {
        public TokenType Operator { get; }
        public IExpression Operand { get; }

        public NegativeExpr(TokenType @operator, IExpression operand, RulePosition position) : base(position)
        {
            Operand = operand;
            Operator = @operator;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
