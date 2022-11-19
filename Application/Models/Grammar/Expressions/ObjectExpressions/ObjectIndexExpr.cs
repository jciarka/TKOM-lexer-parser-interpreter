using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectIndexExpr : ObjectExprBase
    {
        public ExpressionBase Object { get; set; }
        public ExpressionBase IndexExpression { get; }

        public ObjectIndexExpr(ExpressionBase @object, ExpressionBase indexExpression)
        {
            Object = @object;
            IndexExpression = indexExpression;
        }
    }
}
