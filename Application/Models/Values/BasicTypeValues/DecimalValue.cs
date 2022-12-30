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
    public class DecimalValue : IArthmeticValue, IComparableValue, IValue
    {
        public TypeBase Type => new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL);

        public decimal Value { get; }

        public DecimalValue(decimal value)
        {
            Value = value;
        }

        public IValue Add(IValue other)
        {
            return new DecimalValue(Value + ((DecimalValue)other).Value);
        }

        public IValue Sub(IValue other)
        {
            return new DecimalValue(Value - ((DecimalValue)other).Value);
        }

        public IValue Mul(IValue other)
        {
            return new DecimalValue(Value * ((DecimalValue)other).Value);
        }

        public IValue Div(IValue other)
        {
            return new DecimalValue(Value / ((DecimalValue)other).Value);
        }

        public BoolValue Greater(IValue other)
        {
            return new BoolValue(Value > ((DecimalValue)other).Value);
        }

        public BoolValue GreaterEqual(IValue other)
        {
            return new BoolValue(Value >= ((DecimalValue)other).Value);
        }

        public BoolValue Less(IValue other)
        {
            return new BoolValue(Value < ((DecimalValue)other).Value);
        }

        public BoolValue LessEqual(IValue other)
        {
            return new BoolValue(Value <= ((DecimalValue)other).Value);
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value == ((DecimalValue)other).Value);
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue(Value != ((DecimalValue)other).Value);
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            switch (((TypeValue)toType).Value.Type)
            {
                case TypeEnum.INT:
                    return new IntValue((int)Value);
                case TypeEnum.DECIMAL:
                    return new DecimalValue(Value);
                case TypeEnum.CURRENCY:
                    return new CurrencyValue(((TypeValue)toType).Value.Name, Value);
            }

            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
