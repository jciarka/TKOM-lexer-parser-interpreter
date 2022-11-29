using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectMethodExpr : ObjectExprBase
    {
        public ExpressionBase Object { get; set; }
        public string Method { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public ObjectMethodExpr(ExpressionBase @object, string method, IEnumerable<ArgumentBase> arguments)
        {
            Object = @object;
            Method = method;
            Arguments = arguments;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
