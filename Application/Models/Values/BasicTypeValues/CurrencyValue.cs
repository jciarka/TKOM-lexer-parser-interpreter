using Application.Models.ConfigurationParser;
using Application.Models.Exceptions.Interpreter;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.BasicTypeValues
{
    public class CurrencyValue : IArthmeticValue, IComparableValue, IValue
    {
        public TypeBase Type { get; }

        public decimal Value { get; }

        public CurrencyValue(string currenctType)
        {
            Type = new BasicType(currenctType, TypeEnum.CURRENCY);
        }

        public CurrencyValue(string currenctType, decimal value)
        {
            Type = new BasicType(currenctType, TypeEnum.CURRENCY);
            Value = value;
        }

        public IValue Add(IValue other)
        {
            try
            {
                checked { return new CurrencyValue(Type.Name, Value + ((CurrencyValue)other).Value); }
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
                checked { return new CurrencyValue(Type.Name, Value - ((CurrencyValue)other).Value); }
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
                checked { return new CurrencyValue(Type.Name, Value * ((CurrencyValue)other).Value); }
            }
            catch (OverflowException)
            {
                throw new ArthemticOverflowException();
            }
        }

        public IValue Div(IValue other)
        {
            var cOther = (CurrencyValue)other;
            if (cOther.Value == 0)
            {
                throw new ZeroDivisionException();
            }

            return new CurrencyValue(Type.Name, Value / cOther.Value);
        }

        public BoolValue Greater(IValue other)
        {
            return new BoolValue(Value > ((CurrencyValue)other).Value);
        }

        public BoolValue GreaterEqual(IValue other)
        {
            return new BoolValue(Value >= ((CurrencyValue)other).Value);
        }

        public BoolValue Less(IValue other)
        {
            return new BoolValue(Value < ((CurrencyValue)other).Value);
        }

        public BoolValue LessEqual(IValue other)
        {
            return new BoolValue(Value <= ((CurrencyValue)other).Value);
        }

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(Value == ((CurrencyValue)other).Value);
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue(Value != ((CurrencyValue)other).Value);
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
                    var newValue = Value * currencyInfo.currencyConvertions[(Type.Name, ((TypeValue)toType).Value.Name)];
                    return new CurrencyValue(((TypeValue)toType).Value.Name, newValue);
            }

            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Value.ToString() + " " + Type.Name.ToString();
        }
    }
}
