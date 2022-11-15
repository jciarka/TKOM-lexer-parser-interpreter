using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectPropertyExpr : ExpressionBase
    {
        public Identifier Identifier { get; }

        public ObjectPropertyExpr(Identifier identifier)
        {
            Identifier = identifier;
        }
    }
}
