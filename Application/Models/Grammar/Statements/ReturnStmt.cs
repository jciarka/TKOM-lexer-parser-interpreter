using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ReturnStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public IExpression? ReturnExpression { get; }

        public ReturnStmt(RulePosition position, IExpression? returnExpression = null) : base(position)
        {
            ReturnExpression = returnExpression;
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
