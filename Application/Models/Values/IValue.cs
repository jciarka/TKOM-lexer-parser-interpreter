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
    public interface IValue
    {
        TypeBase Type { get; }
        public BoolValue EqualEqual(IValue other);
        public BoolValue BangEqual(IValue other);
        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo);
    }
}
