using Application.Models;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class Scope : IScope
    {
        public Scope? Previous { get; }
        private Dictionary<string, TypeBase> variables;

        public Scope(Scope previous)
        {
            Previous = previous;
            variables = new Dictionary<string, TypeBase>();
        }

        public Scope(IEnumerable<Tuple<string, TypeBase>> locals)
        {
            variables = new Dictionary<string, TypeBase>();

            foreach (var local in locals)
            {
                variables.Add(local.Item1, local.Item2);
            }
        }

        public bool TryFind(string name, out TypeBase? type)
        {
            if (variables.TryGetValue(name, out type))
            {
                return true;
            }

            if (Previous != null)
            {
                return Previous.TryFind(name, out type);
            }

            type = null;
            return false;
        }

        public void Add(string name, TypeBase type)
        {
            if (!variables.TryAdd(name, type))
            {
                throw new VariableRedeclarationException(new CharacterPosition());
            }
        }
    }
}
