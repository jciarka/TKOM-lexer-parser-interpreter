using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectMethodExpr : GrammarRuleBase, IObjectExpression, IVisitable
    {
        public IExpression Object { get; set; }
        public string Method { get; }
        public IEnumerable<IArgument> Arguments { get; }

        public ObjectMethodExpr(IExpression @object, string method, IEnumerable<IArgument> arguments, RulePosition position) : base(position)
        {
            Object = @object;
            Method = method;
            Arguments = arguments;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
