using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ExpressionArgument : ArgumentBase
    {
        public ExpressionBase Expression { get; set; }

        public ExpressionArgument(ExpressionBase expression)
        {
            Expression = expression;
        }
    }
}
