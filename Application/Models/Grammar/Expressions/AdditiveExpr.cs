﻿using Application.Infrastructure.Presenters;
using Application.Models.Tokens;

namespace Application.Models.Grammar
{
    public class AdditiveExpr : ExpressionBase
    {
        public ExpressionBase FirstOperand { get; }
        public IEnumerable<Tuple<TokenType, ExpressionBase>> Operands { get; }

        public AdditiveExpr(ExpressionBase firstOperand, IEnumerable<Tuple<TokenType, ExpressionBase>> operands)
        {
            FirstOperand = firstOperand;
            Operands = operands;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
