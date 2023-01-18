using Application.Models.ConfigurationParser;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.BasicTypeValues
{
    public class TypeValue : IValue
    {
        public TypeBase Type => new TypeType(Value);
        public TypeBase Value { get; }

        public TypeValue()
        {
            Value = new NoneType();
        }

        public TypeValue(TypeBase value)
        {
            Value = value;
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value.Equals(((TypeValue)other).Value));
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue(!Value.Equals(((TypeValue)other).Value));
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Value.Name;
        }
    }
}
