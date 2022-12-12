using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class IfStmt : StatementBase
    {
        public ExpressionBase Condition { get; }
        public StatementBase ThenStatement { get; }
        public StatementBase? ElseStatement { get; }

        public IfStmt(ExpressionBase condition, StatementBase thenStatement, RulePosition position, StatementBase? elseStatement = null) : base(position)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
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
