using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ForeachStmt : StatementBase
    {
        public string? Type { get; }
        public Identifier Identifier { get; }
        public ExpressionBase CollectionExpression { get; }
        public StatementBase Statement { get; }

        public ForeachStmt(StatementBase statement, Identifier identifier, ExpressionBase collectionExpression, string? type = null)
        {
            Type = type;
            Identifier = identifier;
            CollectionExpression = collectionExpression;
            Statement = statement;
        }
    }
}
