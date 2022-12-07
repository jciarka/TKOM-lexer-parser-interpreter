using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionCallTypeAnalyseContext
    {
        public TypeBase? ReturnType { get; }
        public IEnumerable<Tuple<string, TypeBase>> Locals { get; }
        public TypeAnalyseScope Scope { get; private set; }

        public FunctionCallTypeAnalyseContext(FunctionDecl declaration)
        {
            ReturnType = declaration.Type;
            Locals = declaration.Parameters.Select(x => new Tuple<string, TypeBase>(x.Identifier, x.Type));

            Scope = new TypeAnalyseScope(Locals);
        }

        public void PushScope()
        {
            Scope = new TypeAnalyseScope(Scope);
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
