using Application.Infrastructure.Interpreter;
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
    public class Instance : IInstance, IValue
    {
        public IClass Class { get; }

        public Dictionary<string, IValue> Properties { get; set; }

        public TypeBase Type => Class.Type;

        public Instance(IClass @class)
        {
            Class = @class;
            Properties = new();

            foreach (var property in @class.Properties)
            {
                Properties.Add(property.Key, new NullValue());
            }
        }

        public BoolValue BangEqual(IValue other)
        {
            return new BoolValue((IValue)this != other);
        }

        public BoolValue EqualEqual(IValue other)
        {
            // reference to same object
            return new BoolValue((IValue)this == other);
        }

        public IValue GetProperty(string name)
        {
            return Properties[name];
        }

        public void SetProperty(string name, IValue value)
        {
            Properties.TryAdd(name, value);
        }

        public IValue InvokeMethod(string name, IEnumerable<IValue> arguments)
        {
            var argumentTypes = arguments.Select(x => x.Type).ToArray();
            var signature = new FunctionCallExprDescription(name, arguments), out var returnType))

        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotImplementedException();
        }
    }
}
