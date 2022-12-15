using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class IfStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public IExpression Condition { get; }
        public IStatement ThenStatement { get; }
        public IStatement? ElseStatement { get; }

        public IfStmt(IExpression condition, IStatement thenStatement, RulePosition position, IStatement? elseStatement = null) : base(position)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
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
