using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class Literal : GrammarRuleBase, ITerm, IVisitable
    {
        public TypeBase Type { get; set; }

        public bool? BoolValue { get; set; }
        public string? StringValue { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public TypeBase? ValueType { get; set; }

        public Literal(TypeBase typeBase, Token token, RulePosition position) : base(position)
        {
            Type = typeBase;

            BoolValue = token.BoolValue;
            IntValue = token.IntValue;
            DecimalValue = token.DecimalValue;
            StringValue = token.StringValue;
        }

        public Literal(string currencyType, Token numericLiteral, RulePosition position) : base(position)
        {
            Type = new BasicType(currencyType, Types.TypeEnum.CURRENCY);
            DecimalValue = numericLiteral.DecimalValue ?? numericLiteral.IntValue;
        }

        public Literal(TypeBase type, RulePosition position) : base(position)
        {
            Type = new TypeType(type);
            ValueType = type;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
