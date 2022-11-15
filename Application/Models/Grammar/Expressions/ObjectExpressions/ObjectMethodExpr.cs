using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectMethodExpr : ExpressionBase
    {
        public Identifier Identifier { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public ObjectMethodExpr(Identifier identifier, IEnumerable<ArgumentBase> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }
    }
}
