using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public abstract class AssignmentStatementBase : GrammarRuleBase
    {
        public IExpression Expression { get; }

        public AssignmentStatementBase(IExpression expression, RulePosition position) : base(position)
        {
            Expression = expression;
        }
    }

    public class IdentifierAssignmentStatement : AssignmentStatementBase, IStatement
    {
        public Identifier Identifier { get; }

        public IdentifierAssignmentStatement(Identifier identifier, IExpression expression, RulePosition position) : base(expression, position)
        {
            Identifier = identifier;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class PropertyAssignmentStatement : AssignmentStatementBase, IStatement, IVisitable
    {
        public ObjectPropertyExpr Property { get; }

        public PropertyAssignmentStatement(ObjectPropertyExpr property, IExpression expression, RulePosition position) : base(expression, position)
        {
            Property = property;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IndexAssignmentStatement : AssignmentStatementBase, IStatement
    {
        public ObjectIndexExpr IndexExpr { get; }

        public IndexAssignmentStatement(ObjectIndexExpr indexExpr, IExpression expression, RulePosition position) : base(expression, position)
        {
            IndexExpr = indexExpr;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
