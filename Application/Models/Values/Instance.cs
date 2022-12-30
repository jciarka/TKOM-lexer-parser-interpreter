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
    public class Instance : IInstance
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
                Properties.Add(property.Key, ValuesFactory.GetDefaultValue(property.Value));
            }
        }

        public IValue GetProperty(string name)
        {
            return Properties[name];
        }

        public void SetProperty(string name, IValue value)
        {
            Properties[name] = value;
        }

        public IValue InvokeMethod(IInterpreterEngine interpreter, string name, IEnumerable<IValue> arguments)
        {
            var argumentTypes = arguments.Select(x => x.Type).ToArray();
            var signature = new FixedArgumentsFunctionSignature(null!, name, argumentTypes);

            var callable = Class.Methods[signature].Item2;

            return callable.Call(interpreter, new IValue[] { new Reference(this) }.Union(arguments));
        }

        public IValue To(IValue toType, CurrencyTypesInfo currencyInfo)
        {
            throw new NotSupportedException();
        }
    }
}
