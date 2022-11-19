using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class DeclarationStmt : StatementBase
    {
        public string? Type { get; }
        public Identifier Identifier { get; }
        public ExpressionBase? Expression { get; }

        public DeclarationStmt(Identifier identifier, ExpressionBase? expression = null, string? type = null)
        {
            Type = type;
            Identifier = identifier;
            Expression = expression;
        }
    }
}
