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
    public class Reference : IValue
    {
        public TypeBase Type { get; }
        public IInstance? Instance { get; }

        public Reference(TypeBase type)
        {
            Type = type;
            Instance = null;
        }

        public Reference(IInstance instance)
        {
            Type = instance.Type;
            Instance = instance;
        }

        public BoolValue EqualEqual(IValue other)
        {
            var otherRef = other as Reference;

            if (otherRef == null) return new BoolValue(false);

            return new BoolValue(Instance == otherRef.Instance);
        }

        public BoolValue BangEqual(IValue other)
        {
            return EqualEqual(other).Negate();
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotImplementedException();
        }
    }
}
