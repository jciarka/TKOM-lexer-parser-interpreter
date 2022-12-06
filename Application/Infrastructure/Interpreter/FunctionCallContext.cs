using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionCallContext
    {
        public TypeBase? ReturnType { get; }
        public IEnumerable<Tuple<string, TypeBase>> Locals { get; }
        public Scope Scope { get; private set; }

        public FunctionCallContext(FunctionDecl declaration)
        {
            ReturnType = declaration.Type;
            Locals = declaration.Parameters.Select(x => new Tuple<string, TypeBase>(x.Identifier, x.Type));

            Scope = new Scope(Locals);
        }

        public void PushScope()
        {
            Scope = new Scope(Scope);
        }

        public void PopScope()
        {
            if (Scope.Previous == null)
            {
                throw new NotImplementedException();
            }

            Scope = Scope.Previous;
        }
    }
}
