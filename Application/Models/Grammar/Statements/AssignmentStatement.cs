﻿using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
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

        public AssignmentStatementBase(ExpressionBase expression, RulePosition position) : base(position)
        {
            Expression = expression;
        }
    }

    public class IdentifierAssignmentStatement : AssignmentStatementBase
    {
        public Identifier Identifier { get; }

        public IdentifierAssignmentStatement(Identifier identifier, ExpressionBase expression, RulePosition position) : base(expression, position)
        {
            Identifier = identifier;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class PropertyAssignmentStatement : AssignmentStatementBase
    {
        public ObjectPropertyExpr Property { get; }

        public PropertyAssignmentStatement(ObjectPropertyExpr property, ExpressionBase expression, RulePosition position) : base(expression, position)
        {
            Property = property;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IndexAssignmentStatement : AssignmentStatementBase
    {
        public ObjectIndexExpr IndexExpr { get; }

        public IndexAssignmentStatement(ObjectIndexExpr indexExpr, ExpressionBase expression, RulePosition position) : base(expression, position)
        {
            IndexExpr = indexExpr;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
