using Application.Models;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class VariableSet : IVariableSet
    {
        public IVariableSet? Previous { get; }
        private Dictionary<string, IValue> variables;

        public VariableSet(IVariableSet previous)
        {
            Previous = previous;
            variables = new Dictionary<string, IValue>();
        }

        public VariableSet(IEnumerable<Tuple<string, IValue>> locals, IVariableSet? previous = null)
        {
            variables = new Dictionary<string, IValue>();

            foreach (var local in locals)
            {
                variables.Add(local.Item1, local.Item2);
            }

            Previous = previous;
        }

        public bool TryFind(string name, out IValue? value)
        {
            if (variables.TryGetValue(name, out value))
            {
                return true;
            }

            if (Previous != null)
            {
                return Previous.TryFind(name, out value);
            }

            value = null;
            return false;
        }

        public void Set(string name, IValue value)
        {
            variables[name] = value;
        }
    }
}
