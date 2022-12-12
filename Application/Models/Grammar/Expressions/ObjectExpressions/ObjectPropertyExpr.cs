﻿using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ObjectPropertyExpr : ObjectExprBase
    {
        public ExpressionBase Object { get; set; }
        public string Property { get; }

        public ObjectPropertyExpr(ExpressionBase @object, string property, RulePosition position) : base(position)
        {
            Object = @object;
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
}
