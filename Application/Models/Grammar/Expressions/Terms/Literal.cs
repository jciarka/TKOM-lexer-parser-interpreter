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
    public class Literal : TermBase
    {
        public string Type { get; }
        public bool? BoolValue { get; set; }
        public string? StringValue { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public TypeBase? TypeValue { get; set; }

        public Literal(Token token)
        {
            if (token.Type == TokenType.TYPE)
            {
                throw new NotImplementedException();
            }

            BoolValue = token.BoolValue;
            IntValue = token.IntValue;
            DecimalValue = token.DecimalValue;
            Type = token.ValueType!;
            StringValue = token.StringValue;
            TypeValue = null;
        }

        public Literal(Token numericLiteral, string currencyType)
        {
            DecimalValue = numericLiteral.DecimalValue ?? numericLiteral.IntValue;
            Type = currencyType;
        }

        public Literal(string type, bool? boolValue = null, string? stringValue = null, int? intValue = null, decimal? decimalValue = null)
        {
            BoolValue = boolValue;
            StringValue = stringValue;
            IntValue = intValue;
            DecimalValue = decimalValue;
            Type = type;
        }

        public Literal(TypeBase type)
        {
            TypeValue = type;
            Type = "Type";
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
