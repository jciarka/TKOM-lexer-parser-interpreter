using Application.Models.ConfigurationParser;
using Application.Models.Exceptions.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.BasicTypeValues
{
    public class StringValue : IValue
    {
        public TypeBase Type => new BasicType(TypeName.STRING, TypeEnum.STRING);

        public string Value { get; }

        public StringValue()
        {
            Value = "";
        }

        public StringValue(string value)
        {
            Value = value;
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value.Equals(((StringValue)other).Value));
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue(!Value.Equals(((StringValue)other).Value));
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new OperationNotSupportedException($"conversion from string to {((TypeValue)toType).Value.Name}");
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
