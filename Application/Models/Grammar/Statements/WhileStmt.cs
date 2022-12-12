﻿using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
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

        public WhileStmt(ExpressionBase condition, StatementBase statement, RulePosition position) : base(position)
        {
            Condition = condition;
            Statement = statement;
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
