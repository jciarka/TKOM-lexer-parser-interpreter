using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ForeachStmt : StatementBase
    {
        public Parameter Parameter { get; }
        public ExpressionBase CollectionExpression { get; }
        public StatementBase Statement { get; }

        public ForeachStmt(Parameter parameter, ExpressionBase collectionExpression, StatementBase statement)
        {
            Parameter = parameter;
            CollectionExpression = collectionExpression;
            Statement = statement;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
