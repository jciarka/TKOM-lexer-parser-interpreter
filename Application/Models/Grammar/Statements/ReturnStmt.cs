using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ReturnStmt : StatementBase
    {
        public ExpressionBase ReturnExpression { get; }

        public ReturnStmt(ExpressionBase returnExpression)
        {
            ReturnExpression = returnExpression;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
