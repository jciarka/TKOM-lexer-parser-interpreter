using Application.Models.ConfigurationParser;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.BasicTypeValues
{
    public class NullValue : IValue
    {
        public TypeBase Type => new NoneType();

        public BoolValue EqualEqual(IValue other)
        {
            return new BoolValue(
                other.GetType() == typeof(NoneType) ||
                (other.GetType().IsAssignableTo(typeof(Reference)) && ((Reference)other).Instance == null));
        }

        public BoolValue BangEqual(IValue other)
        {
            return EqualEqual(other).Negate();
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotSupportedException();
        }
    }
}
