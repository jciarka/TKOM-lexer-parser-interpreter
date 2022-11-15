using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class WhileStmt : StatementBase
    {
        public ExpressionBase Condition { get; }
        public StatementBase Statement { get; }

        public WhileStmt(ExpressionBase condition, StatementBase statement)
        {
            Condition = condition;
            Statement = statement;
        }
    }
}
