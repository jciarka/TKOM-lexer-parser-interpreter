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
    public class BoolValue : IValue
    {
        public TypeBase Type => new BasicType(TypeName.BOOL, TypeEnum.BOOL);

        public bool Value { get; }

        public BoolValue(bool value)
        {
            Value = value;
        }

        public BoolValue And(IValue other)
        {
            return new BoolValue(Value && ((BoolValue)other).Value);
        }

        public BoolValue Or(IValue other)
        {
            return new BoolValue(Value || ((BoolValue)other).Value);
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value == ((BoolValue)other).Value);
        }

        public BoolValue BangEqual(IValue other)
        {
            return EqualEqual(other).Negate();
        }

        public BoolValue Negate()
        {
            return new BoolValue(!Value);
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
