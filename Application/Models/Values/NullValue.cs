using Application.Models.ConfigurationParser;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values.BasicTypeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values
{
    class NullValue : IValue
    {
        public TypeBase Type => throw new NotImplementedException();

        public BoolValue BangEqual(IValue other)
        {
            throw new NotSupportedException();
        }

        public BoolValue EqualEqual(IValue other)
        {
            throw new NotSupportedException();
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotSupportedException();
        }
    }
}
