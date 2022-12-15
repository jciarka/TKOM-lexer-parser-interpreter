using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class WhileStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public IExpression Condition { get; }
        public IStatement Statement { get; }

        public WhileStmt(IExpression condition, IStatement statement, RulePosition position) : base(position)
        {
            Condition = condition;
            Statement = statement;
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
