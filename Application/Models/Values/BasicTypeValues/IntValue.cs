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
    public class IntValue : IArthmeticValue, IComparableValue, IValue
    {
        public TypeBase Type => new BasicType(TypeName.INT, TypeEnum.INT);

        public int Value { get; }

        public IntValue()
        {
            Value = 0;
        }

        public IntValue(int value)
        {
            Value = value;
        }

        public IValue Add(IValue other)
        {
            try
            {
                checked { return new IntValue(Value + ((IntValue)other).Value); }
            }
            catch (OverflowException)
            {
                throw new ArthemticOverflowException();
            }
        }

        public IValue Sub(IValue other)
        {
            try
            {
                checked { return new IntValue(Value - ((IntValue)other).Value); }
            }
            catch (OverflowException)
            {
                throw new ArthemticOverflowException();
            }
        }

        public IValue Mul(IValue other)
        {
            try
            {
                checked { return new IntValue(Value * ((IntValue)other).Value); }
            }
            catch (OverflowException)
            {
                throw new ArthemticOverflowException();
            }
        }

        public IValue Div(IValue other)
        {
            var iOther = (IntValue)other;
            if (iOther.Value == 0)
            {
                throw new ZeroDivisionException();
            }

            return new IntValue(Value / iOther.Value);
        }

        public BoolValue Greater(IValue other)
        {
            return new BoolValue(Value > ((IntValue)other).Value);
        }

        public BoolValue GreaterEqual(IValue other)
        {
            return new BoolValue(Value >= ((IntValue)other).Value);
        }

        public BoolValue Less(IValue other)
        {
            return new BoolValue(Value < ((IntValue)other).Value);
        }

        public BoolValue LessEqual(IValue other)
        {
            return new BoolValue(Value <= ((IntValue)other).Value);
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value == ((IntValue)other).Value);
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue(Value != ((IntValue)other).Value);
        }

        public IValue To(IValue toType)
        {
            throw new NotSupportedException();
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            switch (((TypeValue)toType).Value.Type)
            {
                case TypeEnum.INT:
                    return new IntValue(Value);
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
