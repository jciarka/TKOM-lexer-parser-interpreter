using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public abstract class AssignmentStatementBase : StatementBase
    {
        public ExpressionBase Expression { get; }

        public AssignmentStatementBase(ExpressionBase expression)
        {
            Expression = expression;
        }
    }

    public class IdentifierAssignmentStatement : AssignmentStatementBase
    {
        public Identifier Identifier { get; }

        public IdentifierAssignmentStatement(Identifier identifier, ExpressionBase expression) : base(expression)
        {
            Identifier = identifier;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }

    public class PropertyAssignmentStatement : AssignmentStatementBase
    {
        public ObjectPropertyExpr Property { get; }

        public PropertyAssignmentStatement(ObjectPropertyExpr property, ExpressionBase expression) : base(expression)
        {
            Property = property;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }

    public class IndexAssignmentStatement : AssignmentStatementBase
    {
        public ObjectIndexExpr IndexExpr { get; }

        public IndexAssignmentStatement(ObjectIndexExpr indexExpr, ExpressionBase expression) : base(expression)
        {
            IndexExpr = indexExpr;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
