using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class DeclarationStmt : StatementBase
    {
        private TypeBase? _type;
        public TypeBase? Type
        {
            get
            {
                return _type;
            }

            set
            {
                if (Type == null)
                {
                    throw new NotImplementedException();
                }

                Type = value;
            }
        }

        public Identifier Identifier { get; }
        public ExpressionBase? Expression { get; }

        public DeclarationStmt(Identifier identifier, ExpressionBase? expression = null, TypeBase? type = null)
        {
            Type = type;
            Identifier = identifier;
            Expression = expression;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
