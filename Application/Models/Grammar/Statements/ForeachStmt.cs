using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ForeachStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public Parameter Parameter { get; }
        public IExpression CollectionExpression { get; }
        public IStatement Statement { get; }

        public ForeachStmt(Parameter parameter, IExpression collectionExpression, IStatement statement, RulePosition position) : base(position)
        {
            Parameter = parameter;
            CollectionExpression = collectionExpression;
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
