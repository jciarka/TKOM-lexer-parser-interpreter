using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectIndexExpr : GrammarRuleBase, IObjectExpression
    {
        public IExpression Object { get; set; }
        public IExpression IndexExpression { get; }

        public ObjectIndexExpr(IExpression @object, IExpression indexExpression, RulePosition position) : base(position)
        {
            Object = @object;
            IndexExpression = indexExpression;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
