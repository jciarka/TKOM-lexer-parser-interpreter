using Application.Infrastructure.Interpreter;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public class DelegateInstance : Instance, ICallable
    {
        public Lambda Lambda { get; }

        public DelegateInstance(TypeBase parametrisingType, Lambda lambda) : base(
            new DelegateClass(parametrisingType))
        {
            Lambda = lambda;
        }

        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            return interpreter.InterpretLambdaCall(Lambda, new Parameter[] { Lambda.Parameter }, arguments);
        }
    }
}
